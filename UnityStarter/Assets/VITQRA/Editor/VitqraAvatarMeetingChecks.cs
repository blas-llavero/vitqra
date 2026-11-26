using System;
using UnityEditor;
using UnityEngine;
using D=Vitqra.VitqraCommunityPolicy;
using M=Vitqra.VitqraAvatarMeeting;
namespace Vitqra
{
    public static class VitqraAvatarMeetingChecks
    {
        [MenuItem("VITQRA/Run Synthetic Avatar Meeting Checks")]
        public static void Run()
        {
            var a=D.Actor.PatientA;var b=D.Actor.ClinicianA;var r=new M(D.Resource.PrivateRoomA);
            Check(r.Join(D.Actor.PatientB,0)==M.Result.Denied&&r.Join(D.Actor.ClinicianB,0)==M.Result.Denied&&r.Join(D.Actor.Ai,0)==M.Result.Denied,"Other identities");
            Check(r.Join(a,4)==M.Result.Invalid&&r.Join(a,0)==M.Result.Ok&&r.Join(b,1)==M.Result.Ok,"Join");
            Check(r.Muted(a)&&r.Muted(b)&&r.SpeakFixture(a,M.Phrase.Greeting)==M.Result.Muted,"Muted default");
            Check(r.SetMuted(a,false)==M.Result.Ok&&r.SpeakFixture(a,M.Phrase.Greeting)==M.Result.Ok,"Speech fixture");
            Check(r.ReadCaption(b).phrase==M.Phrase.Greeting&&r.ReadCaption(D.Actor.PatientB)==null,"Caption scope");
            Check(r.SetMuted(b,false)==M.Result.Ok&&r.SpeakFixture(b,M.Phrase.Ready)==M.Result.Busy,"Single speaker");
            Check(r.AdvanceSecond(4)&&r.ReadCaption(b)!=null&&r.AdvanceSecond(5)&&r.ReadCaption(b)==null,"Expiry");
            Check(r.SpeakFixture(b,M.Phrase.Ready)==M.Result.Ok&&r.SetMuted(b,true)==M.Result.Ok&&r.ReadCaption(a)==null,"Mute clears");
            Check(r.SpeakFixture(a,M.Phrase.ThankYou)==M.Result.Ok,"Second turn");
            r.Leave(a);Check(r.ReadCaption(b)==null&&!r.Joined(a),"Leave clears");
            Check(r.Join(a,3)==M.Result.Ok&&r.Muted(a)&&r.Profile(a)==3,"Rejoin muted");
            Check(r.SetProfile(a,2)==M.Result.Ok&&r.Profile(a)==2,"Profile");
            Check(r.SetMuted(a,false)==M.Result.Ok&&r.SpeakFixture(a,M.Phrase.Greeting)==M.Result.Ok,"Before expiry");
            Check(r.AdvanceDay(30)&&!r.Joined(a)&&!r.Joined(b)&&r.ReadCaption(a)==null,"Room grant expiry");
            Check(!r.AdvanceDay(25)&&!r.AdvanceSecond(0),"No rollback");
            var s=new M(D.Resource.PrivateRoomB);
            Check(s.Join(D.Actor.PatientB,1)==M.Result.Ok&&s.Join(D.Actor.ClinicianB,2)==M.Result.Ok&&s.Join(a,0)==M.Result.Denied,"Room B");
            Check(s.SetMuted(D.Actor.PatientB,false)==M.Result.Ok&&s.AdvanceSecond(86396)&&s.SpeakFixture(D.Actor.PatientB,M.Phrase.Ready)==M.Result.Invalid,"Clock bound");
            s.Clear();Check(!s.Joined(D.Actor.PatientB)&&!s.NetworkEnabled&&!s.MicrophoneEnabled&&!s.RecordingEnabled&&!s.ClinicalRelease,"Closed gates");
            Debug.Log("VITQRA synthetic avatar meeting checks passed.");
        }
        private static void Check(bool ok,string label){if(!ok)throw new InvalidOperationException(label);}
    }
}
