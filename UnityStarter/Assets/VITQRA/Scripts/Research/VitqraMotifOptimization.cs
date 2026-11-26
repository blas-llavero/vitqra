using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Vitqra
{
    // Only the two reviewed algorithms are executable. Model output is inert text.
    public static class VitqraMotifOptimization
    {
        public const string Protocol="motif-ai-revision-v1";
        public const string Task="Find every one-based occurrence of a motif in an abstract DNA string, including overlaps.";
        public const string Choice="First line must be exactly CANDIDATE=NAIVE or CANDIDATE=KMP. Choose only from these two reviewed algorithms. Explain the choice below in the requested language.";
        private static string Number(int n)=>n.ToString(CultureInfo.InvariantCulture);
        public static string Candidate(string reply)
        {
            if(string.IsNullOrEmpty(reply)||reply.Length>6000)return null;
            string first=reply.Split('\n')[0].TrimEnd('\r');
            if(first=="CANDIDATE=NAIVE")return "NAIVE";
            if(first=="CANDIDATE=KMP")return "KMP";
            return null;
        }
        public static VitqraMotifBenchmark.Trial Training()
        {
            // Authored fixture only; never transmit downloaded sample sequences.
            var t=VitqraMotifBenchmark.Evaluate("GATATATGCATATACTT","ATAT");
            return t.passed&&t.cases==71?t:null;
        }
        public static string Feedback(VitqraMotifBenchmark.Trial t)
        {
            if(t==null||!t.passed||t.cases!=71)return null;
            return "Measured on 71 fixed synthetic educational cases: NAIVE="+Number(t.baselineComparisons)+
                ", KMP="+Number(t.kmpComparisons)+" character comparisons including preprocessing. Both matched the reference. "+
                "These are not runtime measurements. Reconsider the candidate using these measurements and "+Choice;
        }
        private static string Sequence(int n,ref uint seed)
        {
            var result=new char[n];
            for(int i=0;i<n;i++){seed=unchecked(seed*1664525u+1013904223u);result[i]="ACGT"[(int)(seed>>30)];}
            return new string(result);
        }
        private static string Reference(string text,string motif)
        {
            var hits=new List<string>();
            for(int start=0;start<=text.Length-motif.Length;)
            {
                int at=text.IndexOf(motif,start,StringComparison.Ordinal);
                if(at<0)break;
                hits.Add(Number(at+1));start=at+1;
            }
            return string.Join(" ",hits);
        }
        public sealed class Holdout
        {
            public int cases,naive,kmp;
            public bool passed;
        }
        public static Holdout EvaluateHoldout()
        {
            // Distinct fixed seed and workload; not shown in the model prompt.
            // Not a hidden competition dataset or independent scientific validation.
            var result=new Holdout();
            uint seed=271828183u;
            for(int i=0;i<48;i++)
            {
                int n=51+i*19,m=1+(i*7)%47;
                string text=Sequence(n,ref seed);
                if(i%6==0)text=new string('A',n-1)+"C";
                string motif=i%6==0?new string('A',m-1)+"C":
                    i%2==0?text.Substring((n-m)/2,m):Sequence(m,ref seed);
                var a=VitqraMotifBenchmark.Search(text,motif,false);
                var b=VitqraMotifBenchmark.Search(text,motif,true);
                result.cases++;
                if(a==null||b==null||a.output!=Reference(text,motif)||b.output!=a.output)return result;
                result.naive+=a.comparisons;result.kmp+=b.comparisons;
            }
            result.passed=true;return result;
        }
        public static string Describe(VitqraResearchDraft.Draft draft)
        {
            if(draft==null||draft.id!=VitqraMotifBenchmark.ChallengeId||draft.optimizationProtocol!=Protocol)return null;
            string first=Candidate(draft.alternatives),second=Candidate(draft.critique);
            var train=Training();var test=EvaluateHoldout();
            if(train==null||!test.passed)return null;
            string winner=test.kmp<test.naive?"KMP":"NAIVE";
            int Count(string candidate)=>candidate=="KMP"?test.kmp:test.naive;
            // Strictly separate measured outcome from the AI preference.
            string change=first==null||second==null?"unavailable":
                Count(second)<Count(first)?"improved":Count(second)>Count(first)?"regressed":"unchanged";
            return "protocol="+Protocol+"\ntraining_cases=71\ntraining_naive="+Number(train.baselineComparisons)+
                "\ntraining_kmp="+Number(train.kmpComparisons)+"\ninitial_ai_choice="+(first??"invalid")+
                "\nrevised_ai_choice="+(second??"invalid")+"\nheld_back_synthetic_cases="+Number(test.cases)+
                "\nheld_back_naive="+Number(test.naive)+"\nheld_back_kmp="+Number(test.kmp)+
                "\nrevision_outcome="+change+"\nselected_by_held_back_comparisons="+winner+
                "\ndraft_sha256="+draft.draftSha256;
        }
    }
}
