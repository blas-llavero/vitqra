using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Vitqra
{
    // Synthetic protocol demonstration only. This is not a disease model.
    // No imports, patient input, persistence, network, AI or clinical release.
    public static class VitqraRetrospective
    {
        public const string Protocol="synthetic-retrospective-v1";
        public const int CohortSize=120, CutoffDay=60, HorizonDay=120;
        private const uint Seed=20261126u;
        private sealed class Row
        {
            public int day,available;
            public int? value;
        }
        private sealed class Subject
        {
            public int id,followup;
            public int? eventDay;
            public Row[] observations;
        }
        private sealed class Example
        {
            public int id,score;
            public bool outcome;
        }
        public sealed class Metrics
        {
            public int TP,FP,TN,FN;
            public int Total=>TP+FP+TN+FN;
            public double? Sensitivity=>Rate(TP,TP+FN);
            public double? Specificity=>Rate(TN,TN+FP);
            public double? PPV=>Rate(TP,TP+FP);
            public double? NPV=>Rate(TN,TN+FN);
            public double? BalancedAccuracy=>Sensitivity.HasValue&&Specificity.HasValue?(Sensitivity.Value+Specificity.Value)/2:(double?)null;
            private static double? Rate(int a,int b)=>b==0?(double?)null:(double)a/b;
        }
        public sealed class Result
        {
            public int threshold,trainAssigned,testAssigned,trainCensored,testCensored,trainMissing,testMissing;
            public Metrics train,test,baseline;
            public string cohortDigest;
            public bool ClinicalRelease=>false;
        }
        private static int Next(ref uint state,int bound)
        {
            state=unchecked(state*1664525u+1013904223u);
            return (int)((state>>8)%(uint)bound);
        }
        private static Subject[] Generate()
        {
            uint s=Seed;
            var result=new Subject[CohortSize];
            for(int id=0;id<CohortSize;id++)
            {
                int latent=Next(ref s,11)-5;
                int start=40+Next(ref s,11)-5;
                int mid=start+latent+Next(ref s,7)-3;
                int end=start+2*latent+Next(ref s,9)-4;
                int delay=Next(ref s,4)==0?7:0;
                bool missingStart=Next(ref s,9)==0;
                bool missingMid=Next(ref s,7)==0;
                bool missingEnd=Next(ref s,8)==0;
                // The outcome is constructed to have a noisy association with latent.
                // It is deliberately NOT independent biological or clinical evidence.
                bool hasEvent=Next(ref s,100)<40+latent*6;
                int eventDay=75+Next(ref s,46);
                int followup=Next(ref s,9)==0?90:120;
                int future=hasEvent?90:10; // Deliberately leaky later feature, excluded.
                result[id]=new Subject{id=id,followup=followup,eventDay=hasEvent&&eventDay<=followup?(int?)eventDay:null,
                    observations=new[]{new Row{day=0,available=0,value=missingStart?(int?)null:start},
                        new Row{day=30,available=30,value=missingMid?(int?)null:mid},
                        new Row{day=60,available=60+delay,value=missingEnd?(int?)null:end},
                        new Row{day=100,available=100,value=future}}};
            }
            return result;
        }
        private static int? Score(Subject subject)
        {
            // Use both event time AND availability time. Missing is not zero.
            var rows=subject.observations.Where(o=>o.day<=CutoffDay&&o.available<=CutoffDay&&o.value.HasValue)
                .OrderBy(o=>o.day).ToArray();
            return rows.Length<2?(int?)null:rows[rows.Length-1].value.Value-rows[0].value.Value;
        }
        private static bool? Outcome(Subject subject)
        {
            if(subject.eventDay.HasValue&&subject.eventDay.Value>CutoffDay&&subject.eventDay.Value<=HorizonDay)return true;
            if(subject.followup<HorizonDay)return null; // Unknown is not negative.
            return false;
        }
        private static Metrics Evaluate(IEnumerable<Example> rows,int? threshold)
        {
            var m=new Metrics();
            foreach(var e in rows)
            {
                bool predicted=threshold.HasValue&&e.score>=threshold.Value;
                if(e.outcome){if(predicted)m.TP++;else m.FN++;}
                else {if(predicted)m.FP++;else m.TN++;}
            }
            return m;
        }
        private static int TrainThreshold(List<Example> train)
        {
            int p=train.Count(e=>e.outcome),n=train.Count-p;
            if(p==0||n==0)throw new InvalidOperationException("Both classes required in development split");
            // Maximize balanced accuracy on development only. Exact integer
            // comparison; ties choose the higher (more conservative) threshold.
            int best=0,bestUtility=-1;
            for(int t=-20;t<=21;t++)
            {
                var m=Evaluate(train,t);
                int utility=m.TP*n+m.TN*p;
                if(utility>=bestUtility){bestUtility=utility;best=t;}
            }
            return best;
        }
        private static string N(int n)=>n.ToString(CultureInfo.InvariantCulture);
        public static Result Run()
        {
            var r=new Result();var train=new List<Example>();var test=new List<Example>();
            var canonical=new StringBuilder(Protocol+"\nseed="+Seed.ToString(CultureInfo.InvariantCulture)+"\ncutoff=60\nhorizon=120\n");
            foreach(var subject in Generate())
            {
                // Fixed, disjoint subject-level split, determined before fitting.
                bool holdout=subject.id%3==0;
                if(holdout)r.testAssigned++;else r.trainAssigned++;
                int? score=Score(subject);bool? outcome=Outcome(subject);
                canonical.Append(N(subject.id)).Append('|').Append(holdout?"holdout":"development").Append('|')
                    .Append(N(subject.followup)).Append('|').Append(subject.eventDay.HasValue?N(subject.eventDay.Value):"none").Append('\n');
                foreach(var row in subject.observations)canonical.Append(N(row.day)).Append('|').Append(N(row.available)).Append('|')
                    .Append(row.value.HasValue?N(row.value.Value):"missing").Append('\n');
                // Exclusions are disjoint: unknown outcome first, then missing features.
                if(!outcome.HasValue){if(holdout)r.testCensored++;else r.trainCensored++;continue;}
                if(!score.HasValue){if(holdout)r.testMissing++;else r.trainMissing++;continue;}
                (holdout?test:train).Add(new Example{id=subject.id,score=score.Value,outcome=outcome.Value});
            }
            if(train.Select(e=>e.id).Intersect(test.Select(e=>e.id)).Any())throw new InvalidOperationException("Subject overlap");
            r.threshold=TrainThreshold(train);
            r.train=Evaluate(train,r.threshold);r.test=Evaluate(test,r.threshold);r.baseline=Evaluate(test,null);
            r.cohortDigest=VitqraLongitudinal.Hash(canonical.ToString());
            return r;
        }
        // Internal regression fixture: future and late-arriving values cannot
        // alter the score. This is a test of implementation, not model validity.
        public static bool CutoffFixture()
        {
            var subject=new Subject{observations=new[]{new Row{day=0,available=0,value=10},new Row{day=30,available=30,value=15},
                new Row{day=60,available=61,value=900},new Row{day=100,available=100,value=999}}};
            int? a=Score(subject);
            subject.observations[2].value=-900;subject.observations[3].value=-999;
            return a==5&&Score(subject)==a;
        }
    }
}
