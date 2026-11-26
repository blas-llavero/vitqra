using System;
using UnityEditor;
using UnityEngine;
using D=Vitqra.VitqraCommunityPolicy;
using M=Vitqra.VitqraValidationCenter;
namespace Vitqra
{
    public static class VitqraValidationCenterChecks
    {
        [MenuItem("VITQRA/Run Synthetic Validation Center Checks")]
        public static void Run()
        {
            var c=new M();var a=D.Actor.Reviewer;var d=M.Reviewer.Domain;var m=M.Reviewer.Methods;
            string first=c.Open(a,d).digest;
            Check(c.Inspect(a,d)==M.Stage.QualityBlocked,"Quality blocked");
            Check(c.Submit(a,d,first,M.Verdict.Favorable,M.Rationale.FixtureChecklist)==M.Result.QualityBlocked,"No incomplete favorable");
            Check(c.Open(D.Actor.Ai,d)==null&&c.Open(a,M.Reviewer.Conflicted)==null&&c.Open(a,M.Reviewer.Unverified)==null,"No reviewer bypass");
            Check(!c.SimulateCorrectedRevision(a,d),"Correction requires request");
            Check(c.Submit(a,d,first,M.Verdict.Revise,M.Rationale.DataIncomplete)==M.Result.Saved,"Request revision");
            Check(c.Inspect(a,d)==M.Stage.RevisionRequested,"Revision status");
            Check(c.Submit(a,m,first,M.Verdict.Favorable,M.Rationale.FixtureChecklist)==M.Result.Closed,"Closed revision");
            Check(c.SimulateCorrectedRevision(a,d),"New revision");
            string second=c.Open(a,d).digest;
            Check(first!=second&&c.Inspect(a,d)==M.Stage.AwaitingReview,"New digest");
            Check(c.Submit(a,d,first,M.Verdict.Favorable,M.Rationale.FixtureChecklist)==M.Result.StaleRevision,"Stale decision");
            Check(c.Submit(a,d,second,M.Verdict.Favorable,M.Rationale.MethodConcern)==M.Result.Invalid,"Rationale");
            Check(c.Submit(a,d,second,M.Verdict.Favorable,M.Rationale.FixtureChecklist)==M.Result.Saved,"Domain");
            Check(c.Submit(a,d,second,M.Verdict.Favorable,M.Rationale.FixtureChecklist)==M.Result.Duplicate,"Distinct reviewer");
            Check(c.Inspect(a,d)==M.Stage.AwaitingReview,"Two required");
            Check(c.Submit(a,m,second,M.Verdict.Favorable,M.Rationale.FixtureChecklist)==M.Result.Saved,"Methods");
            Check(c.Inspect(a,d)==M.Stage.FavorableSimulation&&c.History(a,d).Length==3&&c.Revisions(a,d).Length==2,"History");
            Check(!c.ScientificApproval&&!c.ClinicalApproval&&!c.RegulatoryApproval&&!c.ClinicalRelease&&!c.CertificateIssued&&!c.NetworkEnabled,"No real approval");
            Check(c.Advance(80)&&c.Open(a,d)==null&&c.History(a,d)==null&&c.Revisions(a,d)==null,"Expired");
            Check(!c.Advance(25),"No time rollback");
            var e=new M();
            Check(e.Submit(a,m,e.Open(a,m).digest,M.Verdict.Reject,M.Rationale.EvidenceInsufficient)==M.Result.Saved&&e.Inspect(a,m)==M.Stage.Rejected&&!e.SimulateCorrectedRevision(a,m),"Rejected closed");
            var f=new M();
            Check(f.Submit(a,d,f.Open(a,d).digest,M.Verdict.Revise,M.Rationale.DataIncomplete)==M.Result.Saved&&f.SimulateCorrectedRevision(a,d),"Revision two");
            var old=f.Open(a,d).digest;
            Check(f.Submit(a,d,old,M.Verdict.Favorable,M.Rationale.FixtureChecklist)==M.Result.Saved,"Prior favorable");
            Check(f.Submit(a,m,old,M.Verdict.Revise,M.Rationale.MethodConcern)==M.Result.Saved&&f.SimulateCorrectedRevision(a,m),"Revision three");
            Check(f.Inspect(a,d)==M.Stage.AwaitingReview&&f.Submit(a,d,old,M.Verdict.Favorable,M.Rationale.FixtureChecklist)==M.Result.StaleRevision,"Old votes inactive");
            Check(f.Submit(a,d,f.Open(a,d).digest,M.Verdict.Revise,M.Rationale.MethodConcern)==M.Result.Saved&&!f.SimulateCorrectedRevision(a,d),"Bounded revisions");
            Debug.Log("VITQRA synthetic validation center checks passed.");
        }
        private static void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);}
    }
}
