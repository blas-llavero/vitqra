using System;
using UnityEditor;
using UnityEngine;
using D=Vitqra.VitqraCommunityPolicy;
using T=Vitqra.VitqraTelepresence;
namespace Vitqra
{
    public static class VitqraTelepresenceChecks
    {
        [MenuItem("VITQRA/Run Synthetic Telepresence Checks")]
        public static void Run()
        {
            var a=D.Actor.PatientA;var b=D.Actor.ClinicianA;var t=new T(D.Resource.PrivateRoomA);
            Check(!t.Join(D.Actor.PatientB,0)&&!t.Join(D.Actor.ClinicianB,0)&&!t.Join(D.Actor.Ai,0),"Room isolation");
            Check(!t.Join(a,4)&&t.Join(a,0)&&t.Join(b,1),"Join");
            Check(t.SubmitFixture(a,T.Sample.Neutral,0,0)==T.Result.NoConsent&&t.Read(b,a)==null,"Default deny");
            Check(t.SetConsent(a,true,false)&&t.SubmitFixture(a,T.Sample.Turn,1,0)==T.Result.Saved,"Head only");
            var p=t.Read(b,a);Check(p.head.yaw==45&&p.left==null&&p.right==null,"Minimized pose");
            Check(t.Read(a,a)==null&&t.Read(D.Actor.PatientB,a)==null,"Read scope");
            Check(t.SubmitFixture(a,T.Sample.Neutral,1,0)==T.Result.Replay,"Replay");
            Check(t.AdvanceMs(499)&&t.Read(b,a)!=null&&t.AdvanceMs(500)&&t.Read(b,a)==null,"Expiry");
            Check(t.SubmitFixture(a,T.Sample.Neutral,2,0)==T.Result.Stale&&t.SubmitFixture(a,T.Sample.Neutral,2,501)==T.Result.Invalid,"Capture time");
            Check(t.SetConsent(a,true,true)&&t.SubmitFixture(a,T.Sample.Neutral,2,499)==T.Result.Stale,"Old consent scope");
            Check(t.SubmitFixture(a,T.Sample.Neutral,2,500)==T.Result.Saved&&t.Read(b,a).left!=null,"Both");
            Check(t.SubmitFixture(a,T.Sample.Neutral,3,499)==T.Result.Replay,"Capture order");
            Check(t.SubmitFixture(a,T.Sample.LostHead,3,500)==T.Result.Saved&&t.Read(b,a).head==null&&t.Read(b,a).left!=null,"Head lost");
            Check(t.SubmitFixture(a,T.Sample.LostHands,4,500)==T.Result.Saved&&t.Read(b,a).head!=null&&t.Read(b,a).left==null,"Hands lost");
            Check(t.SubmitFixture(a,T.Sample.InvalidCoordinates,5,500)==T.Result.Invalid&&t.Read(b,a)==null,"Bounds");
            Check(t.SubmitFixture(a,T.Sample.Neutral,6,500)==T.Result.Saved&&t.SetConsent(a,false,true)&&t.Read(b,a)==null,"Changed consent");
            Check(t.SubmitFixture(a,T.Sample.LeftGesture,7,500)==T.Result.Saved&&t.Read(b,a).head==null&&t.Read(b,a).left.y==1500,"Hand only");
            Check(t.SetConsent(a,false,false)&&t.Read(b,a)==null&&t.SetConsent(a,true,true)&&t.Read(b,a)==null,"No revival");
            Check(t.SubmitFixture(a,T.Sample.Neutral,8,500)==T.Result.Saved,"Fresh pose");
            t.Leave(a);Check(t.Read(b,a)==null&&t.Join(a,0)&&t.SubmitFixture(a,T.Sample.Neutral,0,500)==T.Result.NoConsent,"Rejoin resets");
            Check(t.SetConsent(a,true,true)&&t.SubmitFixture(a,T.Sample.Neutral,1,500)==T.Result.Saved,"Rejoin fresh");
            Check(t.AdvanceDay(30)&&!t.Joined(a)&&!t.Joined(b)&&t.Read(b,a)==null,"Access expiry");
            Check(!t.AdvanceDay(20)&&!t.AdvanceMs(499),"Monotonic clocks");
            var u=new T(D.Resource.PrivateRoomB);Check(u.Join(D.Actor.PatientB,0)&&u.Join(D.Actor.ClinicianB,1)&&!u.Join(a,0),"Room B");
            u.Clear();Check(!u.Joined(D.Actor.PatientB)&&!u.NetworkEnabled&&!u.HardwareTrackingEnabled&&!u.RecordingEnabled&&!u.ClinicalRelease,"Release blocked");
            Debug.Log("VITQRA synthetic telepresence checks passed.");
        }
        private static void Check(bool ok,string label){if(!ok)throw new InvalidOperationException(label);}
    }
}
