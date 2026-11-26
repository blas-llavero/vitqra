using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Vitqra
{
    // Closed, fictional research benchmark. No patient input, images, DNA,
    // trained clinical model, individual prediction, network or persistence.
    public static class VitqraMultimodal
    {
        public const string Protocol="synthetic-multimodal-v1";
        public const int Size=600,Cutoff=60,Horizon=120;
        private const uint Seed=271828183u;
        public enum Modality { Temporal, ImageProxy, SymptomProxy, GeneticProxy, Fusion }
        private sealed class Row
        {
            public int modality,day,available;
            public int? value;
        }
        private sealed class Subject
        {
            public int id,followup;
            public int? eventDay;
            public Row[] rows;
        }
        private sealed class Example
        {
            public int id;
            public bool outcome;
            public int[] scores;
        }
        public sealed class Metrics
        {
            public int TP,FP,TN,FN;
            public int Total=>TP+FP+TN+FN;
            public double? Sensitivity=>Rate(TP,TP+FN);
            public double? Specificity=>Rate(TN,TN+FP);
            public double? BalancedAccuracy=>Sensitivity.HasValue&&Specificity.HasValue?(Sensitivity.Value+Specificity.Value)/2:(double?)null;
            public double? PPV=>Rate(TP,TP+FP);
            public double? NPV=>Rate(TN,TN+FN);
            private static double? Rate(int a,int b)=>b==0?(double?)null:(double)a/b;
            // Nominal Wilson 95% intervals, conditional on eligible fixed holdout.
            // Not external validation, calibrated probabilities or prediction intervals.
            public static double[] Wilson(int success,int n)
            {
                if(n<=0||success<0||success>n)return null;
                const double z=1.959963984540054;
                double p=(double)success/n,z2=z*z,den=1+z2/n;
                double center=(p+z2/(2*n))/den;
                double half=z*Math.Sqrt(p*(1-p)/n+z2/(4.0*n*n))/den;
                return new[]{Math.Max(0,center-half),Math.Min(1,center+half)};
            }
        }
        public sealed class Model
        {
            public Modality modality;
            public int threshold;
            public Metrics development,holdout;
        }
        public sealed class Result
        {
            public int developmentAssigned,holdoutAssigned,developmentUnknown,holdoutUnknown,developmentMissing,holdoutMissing;
            public int[] developmentAvailable=new int[4],holdoutAvailable=new int[4];
            public int perturbations;
            public Model[] models;
            public Metrics baseline;
            public string cohortDigest;
            public bool ClinicalRelease=>false;
        }
        private static int Next(ref uint s,int bound)
        {s=unchecked(s*1664525u+1013904223u);return (int)((s>>8)%(uint)bound);}
        private static Row R(int m,int d,int a,int? v)=>new Row{modality=m,day=d,available=a,value=v};
        private static IEnumerable<Subject> Generate()
        {
            uint s=Seed;
            for(int id=0;id<Size;id++)
            {
                int a=Next(ref s,11)-5,b=Next(ref s,11)-5,c=Next(ref s,11)-5,g=Next(ref s,11)-5;
                int start=40+Next(ref s,11)-5;
                int end=start+a+Next(ref s,7)-3;
                int image=b+Next(ref s,7)-3,symptom=c+Next(ref s,7)-3,genetic=g+Next(ref s,7)-3;
                bool ms=Next(ref s,20)==0,me=Next(ref s,15)==0,mi=Next(ref s,10)==0,mc=Next(ref s,12)==0,mg=Next(ref s,15)==0;
                int td=Next(ref s,12)==0?5:0,im=Next(ref s,12)==0?10:0,sy=Next(ref s,15)==0?7:0,ge=Next(ref s,15)==0?65:0;
                // Association deliberately built into the fictional generator.
                int chance=Math.Max(5,Math.Min(95,45+3*a+3*b+3*c+3*g));
                bool ev=Next(ref s,100)<chance;
                int when=75+Next(ref s,46),followup=Next(ref s,12)==0?90:120;
                var rows=new List<Row>{R(0,0,0,ms?(int?)null:start),R(0,60,60+td,me?(int?)null:end),
                    R(1,55,55+im,mi?(int?)null:image),R(2,60,60+sy,mc?(int?)null:symptom),
                    R(3,0,ge,mg?(int?)null:genetic)};
                for(int m=0;m<4;m++)rows.Add(R(m,100,100,ev?999:-999));
                yield return new Subject{id=id,followup=followup,eventDay=ev&&when<=followup?(int?)when:null,rows=rows.ToArray()};
            }
        }
        private static int?[] Scores(Subject s)
        {
            var scores=new int?[4];
            for(int m=0;m<4;m++)
            {
                var rows=s.rows.Where(r=>r.modality==m&&r.day<=Cutoff&&r.available<=Cutoff&&r.value.HasValue).OrderBy(r=>r.day).ToArray();
                if(m==0){if(rows.Length>=2)scores[m]=rows[rows.Length-1].value.Value-rows[0].value.Value;}
                else if(rows.Length>0)scores[m]=rows[rows.Length-1].value.Value;
            }
            return scores;
        }
        private static bool? Outcome(Subject s)
        {
            if(s.eventDay.HasValue&&s.eventDay.Value>Cutoff&&s.eventDay.Value<=Horizon)return true;
            return s.followup<Horizon?(bool?)null:false;
        }
        private static Metrics Evaluate(IEnumerable<Example> rows,int model,int? threshold)
        {
            var m=new Metrics();
            foreach(var e in rows)
            {
                bool pos=threshold.HasValue&&e.scores[model]>=threshold.Value;
                if(e.outcome){if(pos)m.TP++;else m.FN++;}
                else {if(pos)m.FP++;else m.TN++;}
            }
            return m;
        }
        private static int Fit(List<Example> rows,int model)
        {
            int p=rows.Count(e=>e.outcome),n=rows.Count-p;
            if(p==0||n==0)throw new InvalidOperationException("Development requires both classes");
            int best=-33,utility=-1;
            for(int t=-33;t<=33;t++)
            {
                var m=Evaluate(rows,model,t);
                int u=m.TP*n+m.TN*p;
                if(u>=utility){best=t;utility=u;}
            }
            return best;
        }
        private static string N(int n)=>n.ToString(CultureInfo.InvariantCulture);
        public static Result Run()
        {
            var result=new Result();
            var train=new List<Example>();var test=new List<Example>();
            var canonical=new StringBuilder(Protocol+"\nseed="+Seed.ToString(CultureInfo.InvariantCulture)+"\ncutoff=60\nhorizon=120\n");
            foreach(var s in Generate())
            {
                bool hold=s.id%3==0;
                if(hold)result.holdoutAssigned++;else result.developmentAssigned++;
                canonical.Append(N(s.id)).Append('|').Append(hold?"holdout":"development").Append('|').Append(N(s.followup)).Append('|')
                    .Append(s.eventDay.HasValue?N(s.eventDay.Value):"none").Append('\n');
                foreach(var r in s.rows)canonical.Append(N(r.modality)).Append('|').Append(N(r.day)).Append('|').Append(N(r.available)).Append('|')
                    .Append(r.value.HasValue?N(r.value.Value):"missing").Append('\n');
                var scores=Scores(s);
                // Perturb only excluded future or delayed values. Check every subject
                // even if excluded from outcome evaluation.
                foreach(var r in s.rows)if(r.day>Cutoff||r.available>Cutoff)r.value=99999;
                if(!scores.SequenceEqual(Scores(s)))throw new InvalidOperationException("Temporal leakage");
                result.perturbations++;
                var outcome=Outcome(s);
                if(!outcome.HasValue){if(hold)result.holdoutUnknown++;else result.developmentUnknown++;continue;}
                var coverage=hold?result.holdoutAvailable:result.developmentAvailable;
                for(int i=0;i<4;i++)if(scores[i].HasValue)coverage[i]++;
                // Paired complete-case comparison. No imputation; coverage reported
                // independently so excluded subjects cannot silently improve metrics.
                if(scores.Any(x=>!x.HasValue)){if(hold)result.holdoutMissing++;else result.developmentMissing++;continue;}
                var values=scores.Select(x=>x.Value).ToList();values.Add(values.Sum());
                (hold?test:train).Add(new Example{id=s.id,outcome=outcome.Value,scores=values.ToArray()});
            }
            if(train.Select(x=>x.id).Intersect(test.Select(x=>x.id)).Any())throw new InvalidOperationException("Subject overlap");
            if(train.Count+result.developmentUnknown+result.developmentMissing!=result.developmentAssigned||
                test.Count+result.holdoutUnknown+result.holdoutMissing!=result.holdoutAssigned)throw new InvalidOperationException("Denominator mismatch");
            result.models=new Model[5];
            for(int i=0;i<5;i++)
            {
                int t=Fit(train,i);
                result.models[i]=new Model{modality=(Modality)i,threshold=t,development=Evaluate(train,i,t),holdout=Evaluate(test,i,t)};
            }
            result.baseline=Evaluate(test,0,null);
            result.cohortDigest=VitqraLongitudinal.Hash(canonical.ToString());
            return result;
        }
    }
}
