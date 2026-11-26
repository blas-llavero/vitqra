using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Vitqra
{
    // Fixed, reviewed algorithms only. No evaluation of generated code or commands.
    public static class VitqraMotifBenchmark
    {
        public const string ChallengeId="rosalind-subs";
        private const int MaxLength=1000, MaxRecord=8192;
        private static readonly UTF8Encoding Utf8=new UTF8Encoding(false,true);
        public sealed class Result
        {
            public string output;
            public int comparisons;
        }
        public sealed class Trial
        {
            public int cases,baselineComparisons,kmpComparisons,sampleBaseline,sampleKmp;
            public bool passed;
            public string preferred;
        }
        private static string Number(int n)=>n.ToString(CultureInfo.InvariantCulture);
        private static bool Valid(string s)=>!string.IsNullOrEmpty(s)&&s.Length<=MaxLength&&s.All(c=>"ACGT".IndexOf(c)>=0);
        private static Result Finish(List<int> hits,int count)=>new Result{output=string.Join(" ",hits.Select(Number)),comparisons=count};
        public static Result Search(string text,string motif,bool kmp)
        {
            if(!Valid(text)||!Valid(motif)||motif.Length>text.Length)return null;
            int count=0;
            var hits=new List<int>();
            if(!kmp)
            {
                for(int i=0;i<=text.Length-motif.Length;i++)
                {
                    int j=0;
                    while(j<motif.Length)
                    {
                        count++;
                        if(text[i+j]!=motif[j])break;
                        j++;
                    }
                    if(j==motif.Length)hits.Add(i+1);
                }
                return Finish(hits,count);
            }
            // Count prefix-table comparisons as well as search comparisons.
            var prefix=new int[motif.Length];
            int matched=0;
            for(int i=1;i<motif.Length;i++)
            {
                while(true)
                {
                    count++;
                    if(motif[i]==motif[matched]){matched++;break;}
                    if(matched==0)break;
                    matched=prefix[matched-1];
                }
                prefix[i]=matched;
            }
            matched=0;
            for(int i=0;i<text.Length;i++)
            {
                while(true)
                {
                    count++;
                    if(text[i]==motif[matched]){matched++;break;}
                    if(matched==0)break;
                    matched=prefix[matched-1];
                }
                if(matched==motif.Length){hits.Add(i-motif.Length+2);matched=prefix[matched-1];}
            }
            return Finish(hits,count);
        }
        private static bool CheckCase(string text,string motif,string expected,Trial t)
        {
            var a=Search(text,motif,false);var b=Search(text,motif,true);
            t.cases++;
            if(a==null||b==null||a.output!=expected||b.output!=expected)return false;
            t.baselineComparisons+=a.comparisons;t.kmpComparisons+=b.comparisons;
            return true;
        }
        private static string Sequence(int n,ref uint state)
        {
            var chars=new char[n];
            for(int i=0;i<n;i++)
            {
                state=unchecked(state*1664525u+1013904223u);
                chars[i]="ACGT"[(int)(state>>30)];
            }
            return new string(chars);
        }
        // Independent reference using the platform ordinal search and overlapping starts.
        private static string Reference(string text,string motif)
        {
            var hits=new List<int>();
            for(int start=0;start<=text.Length-motif.Length;)
            {
                int index=text.IndexOf(motif,start,StringComparison.Ordinal);
                if(index<0)break;
                hits.Add(index+1);start=index+1;
            }
            return string.Join(" ",hits.Select(Number));
        }
        public static Trial Evaluate(string text,string motif)
        {
            var t=new Trial{preferred="none"};
            var a=Search(text,motif,false);var b=Search(text,motif,true);
            if(a==null||b==null||a.output!=Reference(text,motif)||a.output!=b.output)return t;
            t.sampleBaseline=a.comparisons;t.sampleKmp=b.comparisons;
            // Explicit edge cases and adversarial repeated prefixes.
            if(!CheckCase("AAAAA","AAA","1 2 3",t)||
               !CheckCase("ACGT","ACGT","1",t)||
               !CheckCase("ACGT","T","4",t)||
               !CheckCase("ACGT","AA","",t)||
               !CheckCase("GATATATGCATATACTT","ATAT","2 4 10",t)||
               !CheckCase(new string('A',1000),new string('A',127)+"C","",t)||
               !CheckCase(new string('A',999)+"C",new string('A',127)+"C","873",t))return t;
            uint seed=20261126;
            for(int i=0;i<64;i++)
            {
                string input=Sequence(32+i*13,ref seed);
                int length=1+i%32;
                string pattern=i%2==0?input.Substring((input.Length-length)/2,length):Sequence(length,ref seed);
                if(!CheckCase(input,pattern,Reference(input,pattern),t))return t;
            }
            t.passed=true;
            // Choose only by total character comparisons on the fixed synthetic suite.
            // Tie favors the simpler baseline. This is not a runtime measurement.
            t.preferred=t.kmpComparisons<t.baselineComparisons?"KMP":"Naive";
            return t;
        }
        private static string Read(string path,int max)
        {
            using(var stream=File.OpenRead(path))
            {
                if(stream.Length<=0||stream.Length>max)throw new IOException();
                var data=new byte[(int)stream.Length];int offset=0;
                while(offset<data.Length){int n=stream.Read(data,offset,data.Length-offset);if(n<=0)throw new IOException();offset+=n;}
                if(stream.ReadByte()!=-1)throw new IOException();
                return Utf8.GetString(data);
            }
        }
        private static string PathFor(string root,VitqraChallengeDirectory.Item item)=>
            Path.Combine(VitqraEvidence.Folder(root,item.id,item.url),"motif-benchmark-v1.txt");
        private static string Recompute(string evidence,string results,VitqraChallengeDirectory.Item item)
        {
            if(item==null||item.id!=ChallengeId||!item.educational||item.webCandidate||
                !VitqraEvidenceReview.SourceIdentityMatches(item))return null;
            string folder=VitqraEvidence.Folder(evidence,item.id,item.url);
            if(!File.Exists(Path.Combine(VitqraEvidence.Folder(results,item.id,item.url),"sample-check.json"))||
                VitqraSampleSolver.Run(evidence,results,item)!=VitqraSampleSolver.State.Checked)return null;
            var manifest=VitqraEvidence.ReadVerified(folder,item);
            if(manifest==null)return null;
            var sample=Read(Path.Combine(folder,"public-sample.txt"),8192);
            var asset=manifest.assets.Single(x=>x.name=="public-sample.txt");
            using(var sha=System.Security.Cryptography.SHA256.Create())
                if(BitConverter.ToString(sha.ComputeHash(Utf8.GetBytes(sample))).Replace("-","").ToLowerInvariant()!=asset.sha256)return null;
            var lines=sample.Replace("\r\n","\n").Split('\n');
            if(lines.Length!=2)return null;
            var t=Evaluate(lines[0],lines[1]);
            if(!t.passed)return null;
            var source=manifest.assets.Single(x=>x.name=="source.html.txt");
            return "protocol=motif-comparisons-v1\nseed=20261126\nsource_sha256="+source.sha256+
                "\nsample_sha256="+asset.sha256+"\nsynthetic_cases="+Number(t.cases)+
                "\nnaive_synthetic_comparisons="+Number(t.baselineComparisons)+"\nkmp_synthetic_comparisons="+Number(t.kmpComparisons)+
                "\nnaive_sample_comparisons="+Number(t.sampleBaseline)+"\nkmp_sample_comparisons="+Number(t.sampleKmp)+
                "\nselected_by_synthetic_comparisons="+t.preferred+"\n";
        }
        public static bool Save(string evidence,string results,VitqraChallengeDirectory.Item item)
        {
            string stage=null;
            try
            {
                string record=Recompute(evidence,results,item);
                if(record==null)return false;
                string path=PathFor(results,item);
                if(File.Exists(path))return Read(path,MaxRecord)==record;
                stage=path+".pending-"+Guid.NewGuid().ToString("N");
                File.WriteAllText(stage,record,Utf8);File.Move(stage,path);stage=null;return true;
            }
            catch(Exception){return false;}
            finally{if(stage!=null)try{File.Delete(stage);}catch(Exception){}}
        }
        public static string Load(string evidence,string results,VitqraChallengeDirectory.Item item)
        {
            try
            {
                if(item==null||item.id!=ChallengeId||!File.Exists(PathFor(results,item)))return null;
                string record=Recompute(evidence,results,item);
                return record!=null&&Read(PathFor(results,item),MaxRecord)==record?record:null;
            }
            catch(Exception){return null;}
        }
    }
}
