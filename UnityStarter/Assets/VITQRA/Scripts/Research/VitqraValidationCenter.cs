using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using D=Vitqra.VitqraCommunityPolicy;
using Q=Vitqra.VitqraDataQuality;

namespace Vitqra
{
    // Closed fictional review workflow. No real reviewer identity, credentials,
    // signature, certificate, clinical assessment, storage or network.
    public sealed class VitqraValidationCenter
    {
        public const string Protocol="synthetic-validation-center-v1";
        public enum Reviewer { Domain, Methods, Conflicted, Unverified }
        public enum Verdict { Favorable, Revise, Reject }
        public enum Rationale { FixtureChecklist, DataIncomplete, MethodConcern, EvidenceInsufficient }
        public enum Stage { QualityBlocked, AwaitingReview, RevisionRequested, Rejected, FavorableSimulation }
        public enum Result { Saved, Denied, Invalid, StaleRevision, Duplicate, QualityBlocked, Closed }
        public sealed class Revision
        {
            public readonly int number;
            public readonly Q.Report quality;
            public readonly string digest;
            internal Revision(int n,Q.Scenario scenario)
            {
                number=n;quality=Q.Evaluate(scenario,30);
                digest=VitqraLongitudinal.Hash(Protocol+"\nDossierA\nrevision="+n.ToString(CultureInfo.InvariantCulture)+
                    "\nquality="+quality.digest+"\nmethod=fixed-fictional-method-v1\nscope=synthetic-only\n");
            }
        }
        public sealed class Decision
        {
            public readonly int revision,day;
            public readonly string digest;
            public readonly Reviewer reviewer;
            public readonly Verdict verdict;
            public readonly Rationale rationale;
            internal Decision(Revision r,int day,Reviewer who,Verdict v,Rationale why)
            {revision=r.number;digest=r.digest;this.day=day;reviewer=who;verdict=v;rationale=why;}
        }
        private readonly List<Revision> revisions=new List<Revision>();
        private readonly List<Decision> decisions=new List<Decision>();
        private int day=20;
        public VitqraValidationCenter(){revisions.Add(new Revision(1,Q.Scenario.DifficultDaysOnly));}
        public int Day=>day;
        // Fictional clock advances only. Never use the selected time as authority.
        public bool Advance(int next){if(next<day||next>180)return false;day=next;return true;}
        private Revision Current=>revisions[revisions.Count-1];
        private bool Permitted(D.Actor actor,D.Action action)=>
            D.Evaluate(actor,D.Resource.DossierA,D.Purpose.IndependentReview,action,day).outcome==D.Outcome.AllowSimulation;
        private static bool Assigned(Reviewer reviewer)=>reviewer==Reviewer.Domain||reviewer==Reviewer.Methods;
        public Revision Open(D.Actor actor,Reviewer reviewer)
        {return Assigned(reviewer)&&Permitted(actor,D.Action.Read)?Current:null;}
        public Decision[] History(D.Actor actor,Reviewer reviewer)
        {return Open(actor,reviewer)==null?null:decisions.ToArray();}
        public Revision[] Revisions(D.Actor actor,Reviewer reviewer)
        {return Open(actor,reviewer)==null?null:revisions.ToArray();}
        private Stage Status()
        {
            var active=decisions.Where(x=>x.revision==Current.number).ToArray();
            if(active.Any(x=>x.verdict==Verdict.Reject))return Stage.Rejected;
            if(active.Any(x=>x.verdict==Verdict.Revise))return Stage.RevisionRequested;
            if(Current.quality.gate!=Q.Gate.ReadyForReview)return Stage.QualityBlocked;
            if(active.Count(x=>x.verdict==Verdict.Favorable)==2)return Stage.FavorableSimulation;
            return Stage.AwaitingReview;
        }
        public Stage? Inspect(D.Actor actor,Reviewer reviewer)
        {return Open(actor,reviewer)==null?(Stage?)null:Status();}
        // Reviewers are distinct fixed simulation identities under the existing
        // reviewer role. No caller-selected role or enum authenticates a person.
        public Result Submit(D.Actor actor,Reviewer reviewer,string revisionDigest,Verdict verdict,Rationale rationale)
        {
            if(!Assigned(reviewer)||!Permitted(actor,D.Action.SubmitReviewDraft)||!Permitted(actor,D.Action.Read))return Result.Denied;
            if(!Enum.IsDefined(typeof(Verdict),verdict)||!Enum.IsDefined(typeof(Rationale),rationale))return Result.Invalid;
            if(!string.Equals(revisionDigest,Current.digest,StringComparison.Ordinal))return Result.StaleRevision;
            if(decisions.Any(x=>x.revision==Current.number&&x.reviewer==reviewer))return Result.Duplicate;
            var stage=Status();
            if(stage==Stage.Rejected||stage==Stage.RevisionRequested||stage==Stage.FavorableSimulation)return Result.Closed;
            if(verdict==Verdict.Favorable&&Current.quality.gate!=Q.Gate.ReadyForReview)return Result.QualityBlocked;
            if((verdict==Verdict.Favorable)!=(rationale==Rationale.FixtureChecklist))return Result.Invalid;
            decisions.Add(new Decision(Current,day,reviewer,verdict,rationale));
            return Result.Saved;
        }
        // Explicit editor-only fixture coordinator; NOT an author permission API.
        // No external dossier import. Preserve history and invalidate prior votes.
        public bool SimulateCorrectedRevision(D.Actor actor,Reviewer reviewer)
        {
            if(Open(actor,reviewer)==null||!Permitted(actor,D.Action.SubmitReviewDraft)||
                Status()!=Stage.RevisionRequested||revisions.Count>=3)return false;
            revisions.Add(new Revision(revisions.Count+1,Q.Scenario.Regular));
            return true;
        }
        public bool ScientificApproval=>false;
        public bool ClinicalApproval=>false;
        public bool RegulatoryApproval=>false;
        public bool ClinicalRelease=>false;
        public bool CertificateIssued=>false;
        public bool NetworkEnabled=>false;
    }
}
