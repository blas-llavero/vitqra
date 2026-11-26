using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Vitqra
{
    // Closed synthetic quality fixture only. No intake, clinical interpretation,
    // external dataset, imputation, network, export or actual research approval.
    public static class VitqraDataQuality
    {
        public const string Protocol="synthetic-daily-quality-v1";
        public const int ScheduledDays=30;
        public enum Scenario { Regular, DifficultDaysOnly, Gap, LateArrival, Duplicate, MissingValue, StableOnly, RegularWithExtras, InvalidValue }
        public enum Gate { AwaitingWindow, InvalidData, InsufficientCoverage, ReadyForReview }
        private static readonly int[] Values={2,2,3,1,2,4,7,8,6,3,2,1,2,3,2,3,4,8,9,7,4,3,2,1,2,2,3,4,7,8};
        private sealed class Entry
        {
            public readonly int day,available;
            public readonly bool scheduled;
            public readonly int? value;
            public Entry(int d,int a,bool s,int? v){day=d;available=a;scheduled=s;value=v;}
        }
        public sealed class Report
        {
            public readonly Gate gate;
            public readonly int cutoff,expected,observed,missing,extras,longestGap,low,high,duplicates,invalid;
            public readonly int? meanTimes100;
            public readonly bool observationPatternWarning;
            public readonly string digest;
            internal Report(Gate gate,int cutoff,int expected,int observed,int extras,int gap,int low,int high,int duplicates,int invalid,int? mean,bool warning,string digest)
            {this.gate=gate;this.cutoff=cutoff;this.expected=expected;this.observed=observed;missing=expected-observed;this.extras=extras;
             longestGap=gap;this.low=low;this.high=high;this.duplicates=duplicates;this.invalid=invalid;meanTimes100=mean;
             observationPatternWarning=warning;this.digest=digest;}
            // Passing coverage is not scientific approval or a finding of no bias.
            public bool CanUseForResearch=>false;
            public bool ClinicalRelease=>false;
        }
        private static IEnumerable<Entry> Generate(Scenario scenario)
        {
            for(int day=1;day<=ScheduledDays;day++)
            {
                int v=Values[day-1];
                if(scenario==Scenario.DifficultDaysOnly)
                {
                    if(v>=7)yield return new Entry(day,day,false,v);
                    continue;
                }
                if(scenario==Scenario.Gap&&day>=11&&day<=16)continue;
                int available=scenario==Scenario.LateArrival&&day==24?31:day;
                int? value=scenario==Scenario.MissingValue&&day==15?(int?)null:
                    scenario==Scenario.StableOnly?0:scenario==Scenario.InvalidValue&&day==12?11:v;
                yield return new Entry(day,available,true,value);
                if(scenario==Scenario.Duplicate&&day==12)yield return new Entry(day,day,true,v);
                if(scenario==Scenario.RegularWithExtras&&v>=7)yield return new Entry(day,day,false,v);
            }
        }
        private static string N(int v)=>v.ToString(CultureInfo.InvariantCulture);
        public static Report Evaluate(Scenario scenario,int cutoff)
        {
            if(!Enum.IsDefined(typeof(Scenario),scenario)||cutoff<1||cutoff>31)return null;
            int expected=Math.Min(cutoff,ScheduledDays);
            var visible=Generate(scenario).Where(e=>e.day<=cutoff&&e.available<=cutoff).ToArray();
            var scheduled=visible.Where(e=>e.scheduled).GroupBy(e=>e.day).ToDictionary(g=>g.Key,g=>g.ToArray());
            int invalid=visible.Count(e=>e.day<1||e.day>ScheduledDays||e.available<e.day||e.value.HasValue&&(e.value.Value<0||e.value.Value>10));
            int duplicates=scheduled.Values.Sum(g=>Math.Max(0,g.Length-1));
            int observed=0,gap=0,longest=0,low=0,high=0,sum=0;
            var b=new StringBuilder(Protocol+"\ncutoff="+N(cutoff)+"\n");
            for(int day=1;day<=expected;day++)
            {
                Entry[] group;
                bool has=scheduled.TryGetValue(day,out group);
                bool valid=has&&group.Length==1&&group[0].value.HasValue&&group[0].value.Value>=0&&group[0].value.Value<=10;
                b.Append(N(day)).Append('|');
                if(valid)
                {
                    var e=group[0];int v=e.value.Value;
                    observed++;sum+=v;if(v>=7)high++;else low++;
                    gap=0;b.Append(N(e.available)).Append('|').Append(N(v));
                }
                else {gap++;longest=Math.Max(longest,gap);b.Append(!has?"unobserved":group.Length!=1?"duplicate":"missing_or_invalid");}
                b.Append('\n');
            }
            // Canonicalize only visible entries, including duplicate and extra content,
            // to bind the audit without bringing late/future values into the snapshot.
            foreach(var e in visible.OrderBy(e=>e.day).ThenBy(e=>e.available).ThenBy(e=>e.scheduled).ThenBy(e=>e.value))
                b.Append(e.scheduled?"S|":"E|").Append(N(e.day)).Append('|').Append(N(e.available)).Append('|')
                 .Append(e.value.HasValue?N(e.value.Value):"missing").Append('\n');
            int extras=visible.Count(e=>!e.scheduled);
            Gate gate=invalid>0||duplicates>0?Gate.InvalidData:
                cutoff<ScheduledDays?Gate.AwaitingWindow:observed!=ScheduledDays?Gate.InsufficientCoverage:Gate.ReadyForReview;
            // A warning is descriptive, not a statistical test of MNAR or proof of bias.
            bool warning=extras>0||observed<expected||low==0||high==0;
            return new Report(gate,cutoff,expected,observed,extras,longest,low,high,duplicates,invalid,
                observed==0?(int?)null:(sum*100+observed/2)/observed,warning,VitqraLongitudinal.Hash(b.ToString()));
        }
    }
}
