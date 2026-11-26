using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using D=Vitqra.VitqraCommunityPolicy;
using M=Vitqra.VitqraDailyDiary;

namespace Vitqra
{
    public static class VitqraDailyDiaryChecks
    {
        [MenuItem("VITQRA/Run Synthetic Daily Diary Checks")]
        public static void Run()
        {
            var a=D.Actor.PatientA;
            var d=new M(D.Resource.RecordA,M.Protocol.DailyDemo);
            Check(!d.ReminderOptIn&&!d.DeviceOptIn,"Opt-in defaults");
            Check(!d.PollReminder(a,1080),"No unsolicited reminder");
            Check(d.SetReminder(a,true,1080)&&!d.PollReminder(a,1079)&&d.PollReminder(a,1080)&&!d.PollReminder(a,1081),"One due reminder");
            Check(d.Record(D.Actor.PatientB,M.State.Worse,false)==M.WriteResult.Denied,"Other patient");
            Check(d.Record(D.Actor.ClinicianA,M.State.Worse,false)==M.WriteResult.Denied,"Clinician cannot impersonate");
            Check(d.Record(a,M.State.NoSymptoms,false)==M.WriteResult.Saved&&d.Record(a,M.State.Worse,false)==M.WriteResult.AlreadyRecorded,"No silent overwrite");
            Check(d.Record(a,M.State.Worse,true)==M.WriteResult.Saved,"Separate extra");
            Check(d.ImportSimulatedDevice(a)==M.WriteResult.NotAuthorized,"Device default denied");
            Check(d.SetDeviceAuthorization(a,true)&&d.ImportSimulatedDevice(a)==M.WriteResult.Saved&&d.ImportSimulatedDevice(a)==M.WriteResult.AlreadyRecorded,"Authorized single import");
            Check(d.SetDeviceAuthorization(a,false)&&d.Advance(a,2)&&d.ImportSimulatedDevice(a)==M.WriteResult.NotAuthorized,"Revocation");
            Check(d.Record(a,M.State.Unsure,false)==M.WriteResult.Saved&&d.Advance(a,3)&&d.Record(a,M.State.UnableToMeasure,false)==M.WriteResult.Saved,"Unknown and unable");
            Check(d.Advance(a,5)&&d.Record(a,M.State.Better,false)==M.WriteResult.Saved,"Unanswered day");
            Check(d.Advance(a,6)&&d.Record(a,M.State.Same,false)==M.WriteResult.Saved&&d.Advance(a,7)&&d.Record(a,M.State.Worse,false)==M.WriteResult.Saved,"All six states");
            var c=d.Inspect(a,7);
            Check(c.expected==7&&c.responded==6&&c.usableState==4&&c.unknown==1&&c.unable==1&&c.unanswered==1&&c.extras==1&&c.device==1,"Separate denominators");
            Check(c.distribution.SequenceEqual(new[]{1,1,1,1,1,1}),"Preserved frequencies");
            Check(d.Inspect(a,1).responded==1&&d.Inspect(a,8)==null&&d.Snapshot(D.Actor.Ai,7)==null&&d.Snapshot(D.Actor.PatientB,7)==null,"Scope and cutoff");
            Check(!d.Advance(a,1)&&!d.Advance(a,31),"Clock bounds");
            var alternate=new M(D.Resource.RecordA,M.Protocol.AlternateDayDemo);
            Check(alternate.SetReminder(a,true,1080)&&alternate.Advance(a,2)&&!alternate.PollReminder(a,1080)&&alternate.Record(a,M.State.NoSymptoms,false)==M.WriteResult.Invalid,"Protocol cadence");
            Check(alternate.Record(a,M.State.NoSymptoms,true)==M.WriteResult.Saved&&alternate.Advance(a,30),"Extras do not fill slots");
            Check(alternate.Inspect(a,30).expected==15&&alternate.Inspect(a,30).responded==0,"Alternate denominator");
            var stable=new M(D.Resource.RecordA,M.Protocol.DailyDemo);
            for(int i=1;i<=30;i++){Check(stable.Advance(a,i),"Advance");Check(stable.Record(a,M.State.NoSymptoms,false)==M.WriteResult.Saved,"Stable entries");}
            Check(stable.Inspect(a,30).distribution.SequenceEqual(new[]{30,0,0,0,0,0}),"No artificial balancing");
            Check(!stable.CanUseForResearch&&!stable.ClinicalRelease&&!stable.NetworkEnabled,"Release gates");
            Debug.Log("VITQRA synthetic daily diary checks passed.");
        }
        private static void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);}
    }
}
