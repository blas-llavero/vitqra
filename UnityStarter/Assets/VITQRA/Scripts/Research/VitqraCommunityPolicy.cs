using System;
using System.Globalization;

namespace Vitqra
{
    // Closed fictional policy demonstration. No real identities, data payloads,
    // tokens, authentication, persistence or network. Never a security boundary.
    public static class VitqraCommunityPolicy
    {
        public const string Protocol="synthetic-community-policy-v1";
        public enum Actor { PatientA, PatientB, ClinicianA, ClinicianB, Researcher, Industry, Reviewer, Peer, Moderator, UnverifiedClinician, Ai, Administrator }
        public enum Resource { RecordA, RecordB, StudyA, StudyB, AggregateA, AggregateB, DossierA, DossierB, CommunityA, CommunityB, PrivateRoomA, PrivateRoomB, PublicEducation }
        public enum Purpose { Care, Research, Collaboration, IndependentReview, PeerSupport, Education }
        public enum Action { Read, AddOwnObservation, SubmitReviewDraft, Post, Moderate, Join, Export, RecordSession, Approve, GrantAccess }
        public enum Outcome { Deny, AllowSimulation }
        public enum Reason { Invalid, NoIndependentAuthority, NoRole, NoResourceGrant, NotYetActive, Expired, Revoked, Granted }
        public sealed class Decision
        {
            public readonly Outcome outcome;
            public readonly Reason reason;
            internal Decision(Outcome o,Reason r){outcome=o;reason=r;}
        }
        private sealed class Grant
        {
            public readonly Actor actor;
            public readonly Resource resource;
            public readonly Purpose purpose;
            public readonly Action action;
            public readonly int start,end,revoked;
            public Grant(Actor a,Resource r,Purpose p,Action op,int first,int last,int revoke)
            {actor=a;resource=r;purpose=p;action=op;start=first;end=last;revoked=revoke;}
        }
        private static readonly Grant[] Grants={
            new Grant(Actor.PatientA,Resource.RecordA,Purpose.Care,Action.Read,0,90,181),
            new Grant(Actor.PatientA,Resource.RecordA,Purpose.Care,Action.AddOwnObservation,0,90,181),
            new Grant(Actor.PatientB,Resource.RecordB,Purpose.Care,Action.Read,0,90,181),
            new Grant(Actor.PatientB,Resource.RecordB,Purpose.Care,Action.AddOwnObservation,0,90,181),
            new Grant(Actor.ClinicianA,Resource.RecordA,Purpose.Care,Action.Read,10,70,50),
            new Grant(Actor.ClinicianB,Resource.RecordB,Purpose.Care,Action.Read,10,70,181),
            new Grant(Actor.Researcher,Resource.StudyA,Purpose.Research,Action.Read,5,65,181),
            new Grant(Actor.Researcher,Resource.AggregateA,Purpose.Research,Action.Read,5,65,181),
            new Grant(Actor.Industry,Resource.AggregateA,Purpose.Collaboration,Action.Read,15,60,181),
            new Grant(Actor.Reviewer,Resource.DossierA,Purpose.IndependentReview,Action.Read,20,80,181),
            new Grant(Actor.Reviewer,Resource.DossierA,Purpose.IndependentReview,Action.SubmitReviewDraft,20,80,181),
            new Grant(Actor.PatientA,Resource.CommunityA,Purpose.PeerSupport,Action.Join,0,90,181),
            new Grant(Actor.PatientA,Resource.CommunityA,Purpose.PeerSupport,Action.Read,0,90,181),
            new Grant(Actor.PatientA,Resource.CommunityA,Purpose.PeerSupport,Action.Post,0,90,181),
            new Grant(Actor.Peer,Resource.CommunityA,Purpose.PeerSupport,Action.Join,0,90,181),
            new Grant(Actor.Peer,Resource.CommunityA,Purpose.PeerSupport,Action.Read,0,90,181),
            new Grant(Actor.Peer,Resource.CommunityA,Purpose.PeerSupport,Action.Post,0,90,181),
            new Grant(Actor.Moderator,Resource.CommunityA,Purpose.PeerSupport,Action.Read,0,90,181),
            new Grant(Actor.Moderator,Resource.CommunityA,Purpose.PeerSupport,Action.Moderate,0,90,181),
            new Grant(Actor.PatientA,Resource.PrivateRoomA,Purpose.Care,Action.Join,20,30,181),
            new Grant(Actor.ClinicianA,Resource.PrivateRoomA,Purpose.Care,Action.Join,20,30,181),
            new Grant(Actor.PatientB,Resource.PrivateRoomB,Purpose.Care,Action.Join,20,30,181),
            new Grant(Actor.ClinicianB,Resource.PrivateRoomB,Purpose.Care,Action.Join,20,30,181),
            new Grant(Actor.PatientA,Resource.PublicEducation,Purpose.Education,Action.Read,0,181,181),
            new Grant(Actor.PatientB,Resource.PublicEducation,Purpose.Education,Action.Read,0,181,181),
            new Grant(Actor.Peer,Resource.PublicEducation,Purpose.Education,Action.Read,0,181,181),
        };
        private static Decision Deny(Reason r)=>new Decision(Outcome.Deny,r);
        private static bool RoleAllows(Actor a,Resource r,Purpose p,Action op)
        {
            bool patient=a==Actor.PatientA||a==Actor.PatientB;
            bool clinician=a==Actor.ClinicianA||a==Actor.ClinicianB;
            if(r==Resource.PublicEducation)return p==Purpose.Education&&op==Action.Read&&(patient||a==Actor.Peer);
            if(r==Resource.RecordA||r==Resource.RecordB)return p==Purpose.Care&&
                ((patient&&(op==Action.Read||op==Action.AddOwnObservation))||(clinician&&op==Action.Read));
            if(r==Resource.PrivateRoomA||r==Resource.PrivateRoomB)return p==Purpose.Care&&op==Action.Join&&(patient||clinician);
            if(r==Resource.StudyA||r==Resource.StudyB)return p==Purpose.Research&&a==Actor.Researcher&&op==Action.Read;
            if(r==Resource.AggregateA||r==Resource.AggregateB)return op==Action.Read&&
                ((a==Actor.Researcher&&p==Purpose.Research)||(a==Actor.Industry&&p==Purpose.Collaboration));
            if(r==Resource.DossierA||r==Resource.DossierB)return p==Purpose.IndependentReview&&a==Actor.Reviewer&&
                (op==Action.Read||op==Action.SubmitReviewDraft);
            if((r==Resource.CommunityA||r==Resource.CommunityB)&&p==Purpose.PeerSupport)
                return ((patient||a==Actor.Peer)&&(op==Action.Join||op==Action.Read||op==Action.Post))||
                    (a==Actor.Moderator&&(op==Action.Read||op==Action.Moderate));
            return false;
        }
        public static Decision Evaluate(Actor a,Resource r,Purpose p,Action op,int day)
        {
            if(!Enum.IsDefined(typeof(Actor),a)||!Enum.IsDefined(typeof(Resource),r)||
               !Enum.IsDefined(typeof(Purpose),p)||!Enum.IsDefined(typeof(Action),op)||day<0||day>180)
                return Deny(Reason.Invalid);
            if(a==Actor.UnverifiedClinician||a==Actor.Ai||a==Actor.Administrator)
                return Deny(Reason.NoIndependentAuthority);
            if(!RoleAllows(a,r,p,op))return Deny(Reason.NoRole);
            foreach(var g in Grants)
            {
                if(g.actor!=a||g.resource!=r||g.purpose!=p||g.action!=op)continue;
                if(day<g.start)return Deny(Reason.NotYetActive);
                if(day>=g.revoked)return Deny(Reason.Revoked);
                if(day>=g.end)return Deny(Reason.Expired);
                return new Decision(Outcome.AllowSimulation,Reason.Granted);
            }
            return Deny(Reason.NoResourceGrant);
        }
        // Fictional decision summary only; no persistent audit or sensitive payload.
        public static string Describe(Actor a,Resource r,Purpose p,Action op,int day)
        {
            var d=Evaluate(a,r,p,op,day);
            if(d.reason==Reason.Invalid)return Protocol+"|invalid|deny";
            return Protocol+"|"+a+"|"+r+"|"+p+"|"+op+"|"+day.ToString(CultureInfo.InvariantCulture)+"|"+d.outcome+"|"+d.reason;
        }
        public static bool ClinicalRelease=>false;
        public static bool NetworkEnabled=>false;
    }
}
