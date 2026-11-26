using System;
using System.Collections.Generic;
using System.Linq;
using D=Vitqra.VitqraCommunityPolicy;

namespace Vitqra
{
    // Interactive fictional diary, memory only. Never enter real health data.
    // No device API, clinical interpretation, notification service or persistence.
    public sealed class VitqraDailyDiary
    {
        public enum State { NoSymptoms, Better, Same, Worse, Unsure, UnableToMeasure }
        public enum Protocol { DailyDemo, AlternateDayDemo }
        public enum Source { ScheduledSelfReport, ExtraSelfReport, SimulatedDevice }
        public enum WriteResult { Saved, Denied, Invalid, AlreadyRecorded, NotAuthorized }
        public sealed class Entry
        {
            public readonly int day,availableDay;
            public readonly Source source;
            public readonly State? state;
            public readonly int? unitlessValue;
            internal Entry(int d,int a,Source s,State? x,int? v)
            {day=d;availableDay=a;source=s;state=x;unitlessValue=v;}
        }
        public sealed class Coverage
        {
            public readonly int expected,responded,usableState,unknown,unable,unanswered,extras,device;
            public readonly int[] distribution;
            internal Coverage(int e,int r,int u,int k,int n,int x,int d,int[] counts)
            {expected=e;responded=r;usableState=u;unknown=k;unable=n;unanswered=e-r;extras=x;device=d;distribution=counts;}
        }
        public readonly Protocol protocol;
        private readonly D.Resource resource;
        private readonly D.Actor owner;
        private readonly List<Entry> entries=new List<Entry>();
        private bool reminders,deviceAuthorized;
        private int minute=18*60,lastReminderDay=-1,currentDay=1;
        public int CurrentDay=>currentDay;
        public bool ReminderOptIn=>reminders;
        public bool DeviceOptIn=>deviceAuthorized;
        public int ReminderMinute=>minute;
        public VitqraDailyDiary(D.Resource record,Protocol p)
        {
            if(record!=D.Resource.RecordA&&record!=D.Resource.RecordB||!Enum.IsDefined(typeof(Protocol),p))
                throw new ArgumentException("Invalid fictional diary");
            resource=record;protocol=p;
            owner=record==D.Resource.RecordA?D.Actor.PatientA:D.Actor.PatientB;
        }
        public bool Scheduled(int day)=>day>=1&&day<=30&&(protocol==Protocol.DailyDemo||day%2==1);
        private bool OwnWrite(D.Actor actor)=>actor==owner&&D.Evaluate(actor,resource,D.Purpose.Care,D.Action.AddOwnObservation,currentDay).outcome==D.Outcome.AllowSimulation;
        private bool Read(D.Actor actor)=>D.Evaluate(actor,resource,D.Purpose.Care,D.Action.Read,currentDay).outcome==D.Outcome.AllowSimulation;
        // Only advance the fictional clock, no retroactive editing or reset of history.
        public bool Advance(D.Actor actor,int day)
        {
            if(!OwnWrite(actor)||day<currentDay||day>30)return false;
            currentDay=day;return true;
        }
        public bool SetReminder(D.Actor actor,bool enabled,int localMinute)
        {
            if(!OwnWrite(actor)||localMinute<0||localMinute>=1440)return false;
            reminders=enabled;minute=localMinute;return true;
        }
        public bool SetDeviceAuthorization(D.Actor actor,bool enabled)
        {
            if(!OwnWrite(actor))return false;
            deviceAuthorized=enabled;return true;
        }
        public WriteResult Record(D.Actor actor,State state,bool extra)
        {
            if(!OwnWrite(actor))return WriteResult.Denied;
            if(!Enum.IsDefined(typeof(State),state)||!extra&&!Scheduled(currentDay))return WriteResult.Invalid;
            var source=extra?Source.ExtraSelfReport:Source.ScheduledSelfReport;
            if(entries.Any(e=>e.day==currentDay&&e.source==source))return WriteResult.AlreadyRecorded;
            entries.Add(new Entry(currentDay,currentDay,source,state,null));return WriteResult.Saved;
        }
        // Fixed unitless simulated source. Never takes an actual sensor reading.
        // Revoking consent prevents future imports; historical entries are not erased.
        public WriteResult ImportSimulatedDevice(D.Actor actor)
        {
            if(!OwnWrite(actor))return WriteResult.Denied;
            if(!deviceAuthorized)return WriteResult.NotAuthorized;
            if(entries.Any(e=>e.day==currentDay&&e.source==Source.SimulatedDevice))return WriteResult.AlreadyRecorded;
            entries.Add(new Entry(currentDay,currentDay,Source.SimulatedDevice,null,40+currentDay%9));
            return WriteResult.Saved;
        }
        // Simulated in-app reminder, not a system notification. At most once
        // per scheduled day, after chosen local minute, no missed-day catch-up.
        public bool PollReminder(D.Actor actor,int localMinute)
        {
            if(!OwnWrite(actor)||!reminders||!Scheduled(currentDay)||localMinute<minute||localMinute>=1440||
                lastReminderDay==currentDay||entries.Any(e=>e.day==currentDay&&e.source==Source.ScheduledSelfReport))return false;
            lastReminderDay=currentDay;return true;
        }
        public Entry[] Snapshot(D.Actor actor,int cutoff)
        {
            if(!Read(actor)||cutoff<1||cutoff>currentDay)return null;
            return entries.Where(e=>e.day<=cutoff&&e.availableDay<=cutoff).OrderBy(e=>e.day).ThenBy(e=>e.source).ToArray();
        }
        public Coverage Inspect(D.Actor actor,int cutoff)
        {
            var rows=Snapshot(actor,cutoff);if(rows==null)return null;
            int expected=Enumerable.Range(1,cutoff).Count(Scheduled);
            var scheduled=rows.Where(e=>e.source==Source.ScheduledSelfReport).ToArray();
            int[] counts=new int[6];foreach(var e in scheduled)counts[(int)e.state.Value]++;
            return new Coverage(expected,scheduled.Length,counts.Take(4).Sum(),counts[4],counts[5],
                rows.Count(e=>e.source==Source.ExtraSelfReport),rows.Count(e=>e.source==Source.SimulatedDevice),counts);
        }
        public bool CanUseForResearch=>false;
        public bool ClinicalRelease=>false;
        public bool NetworkEnabled=>false;
    }
}
