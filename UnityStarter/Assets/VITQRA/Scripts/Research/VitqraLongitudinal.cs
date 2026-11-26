using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Vitqra
{
    // Closed synthetic fixture. No patient data, external input or network access.
    public static class VitqraLongitudinal
    {
        public const string Protocol="synthetic-longitudinal-v1";
        public const int Participants=3, MaxDay=180;
        public static readonly DateTimeOffset Epoch=new DateTimeOffset(2026,1,1,0,0,0,TimeSpan.Zero);
        public enum Feature { A, B }
        public sealed class Observation
        {
            public readonly int participant,observedDay,availableDay;
            public readonly Feature feature;
            public readonly int? value;
            public readonly bool researchAllowed;
            internal Observation(int p,int observed,int available,Feature f,int? v,bool allowed)
            {participant=p;observedDay=observed;availableDay=available;feature=f;value=v;researchAllowed=allowed;}
        }
        public sealed class Snapshot
        {
            public readonly int participant,cutoffDay;
            public readonly ReadOnlyCollection<Observation> observations;
            public readonly string digest;
            internal Snapshot(int p,int day,Observation[] rows)
            {
                participant=p;cutoffDay=day;observations=Array.AsReadOnly(rows);
                var b=new StringBuilder(Protocol+"\n");
                b.Append(N(p)).Append('|').Append(N(day)).Append('\n');
                foreach(var o in rows)b.Append(o.feature).Append('|').Append(N(o.observedDay)).Append('|')
                    .Append(N(o.availableDay)).Append('|').Append(o.value.HasValue?N(o.value.Value):"missing")
                    .Append("|synthetic_fixture|research_only\n");
                digest=Hash(b.ToString());
            }
        }
        internal static string Hash(string text)
        {
            using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(new UTF8Encoding(false,true).GetBytes(text))).Replace("-","").ToLowerInvariant();
        }
        private static string N(int n)=>n.ToString(CultureInfo.InvariantCulture);
        public static string Date(int day)=>Epoch.AddDays(day).ToString("yyyy-MM-dd",CultureInfo.InvariantCulture);
        public static string Subject(int p)=>"SYN-"+(p+1).ToString("D3",CultureInfo.InvariantCulture);
        private static IEnumerable<Observation> Generate()
        {
            for(int p=0;p<Participants;p++)for(int i=0;i<7;i++)
            {
                int day=i*30;
                yield return new Observation(p,day,day+(i==2?12:0),Feature.A,i==3&&p==1?(int?)null:20+p*4+i*(p+1)*3,i!=4);
                yield return new Observation(p,day,day+(i==1?5:0),Feature.B,i==2?(int?)null:70-p*3-i*2,true);
            }
        }
        public static Snapshot At(int participant,int cutoffDay)
        {
            if(participant<0||participant>=Participants||cutoffDay<0||cutoffDay>MaxDay)return null;
            var rows=Generate().Where(o=>o.participant==participant&&o.researchAllowed&&o.observedDay<=cutoffDay&&o.availableDay<=cutoffDay)
                .OrderBy(o=>o.observedDay).ThenBy(o=>o.feature).ToArray();
            return new Snapshot(participant,cutoffDay,rows);
        }
        public static int? Change(Snapshot snapshot,Feature feature)
        {
            if(snapshot==null)return null;
            var rows=snapshot.observations.Where(o=>o.feature==feature&&o.value.HasValue).ToArray();
            return rows.Length<2?(int?)null:rows[rows.Length-1].value.Value-rows[0].value.Value;
        }
        public static bool ClinicalRelease=>false;
    }
}
