using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Vitqra
{
    // Educational sample checks only. Never executes downloaded or generated code.
    // Does not authorize a research job or a competition submission.
    public static class VitqraSampleSolver
    {
        public enum State { Unsupported, EvidenceRequired, OutputUnavailable, Mismatch, Checked, StorageFailure }
        [Serializable] public sealed class Checkpoint
        {
            public int version=1;
            public string scope="public_educational_sample_only";
            public string challengeId,sourceUrl,inputSha256,sourceSha256,expectedSha256,resultSha256;
            public string result,checkedUtc;
        }
        public static bool Supports(string id)
        {
            switch(id)
            {
                case "rosalind-dna":case "rosalind-rna":case "rosalind-revc":
                case "rosalind-hamm":case "rosalind-subs":return true;
                default:return false;
            }
        }
        public static string Solve(string id,string sample)
        {
            if(!Supports(id)||sample==null||sample.Length>2048||
               VitqraEvidenceReview.CheckSample(id,sample)!=VitqraEvidenceReview.SampleState.FormatMatches)return null;
            var lines=sample.Replace("\r\n","\n").Split('\n');
            string s=lines[0];
            if(s.Length>1000||lines.Any(x=>x.Length>1000))return null;
            switch(id)
            {
                case "rosalind-dna":
                    var counts=new int[4];
                    foreach(char c in s)counts["ACGT".IndexOf(c)]++;
                    return string.Join(" ",counts.Select(x=>x.ToString(CultureInfo.InvariantCulture)));
                case "rosalind-rna":return s.Replace('T','U');
                case "rosalind-revc":
                    var complement=new char[s.Length];
                    for(int i=0;i<s.Length;i++)complement[i]="TGCA"["ACGT".IndexOf(s[s.Length-1-i])];
                    return new string(complement);
                case "rosalind-hamm":
                    int differences=0;
                    for(int i=0;i<s.Length;i++)if(s[i]!=lines[1][i])differences++;
                    return differences.ToString(CultureInfo.InvariantCulture);
                case "rosalind-subs":
                    var result=new StringBuilder();
                    string motif=lines[1];
                    for(int i=0;i<=s.Length-motif.Length;i++)
                        if(string.CompareOrdinal(s,i,motif,0,motif.Length)==0)
                        {if(result.Length>0)result.Append(' ');result.Append((i+1).ToString(CultureInfo.InvariantCulture));}
                    return result.ToString();
                default:return null;
            }
        }
        public static string ExpectedOutput(string html)
        {
            if(string.IsNullOrEmpty(html)||html.Length>VitqraEvidence.MaxHtml)return null;
            try
            {
                var matches=Regex.Matches(html,@"<h([1-6])\b[^>]*>\s*Sample Output\s*</h\1>\s*<pre\b[^>]*>(.*?)</pre>",
                    RegexOptions.IgnoreCase|RegexOptions.Singleline|RegexOptions.CultureInvariant,TimeSpan.FromSeconds(1));
                if(matches.Count!=1)return null;
                string value=WebUtility.HtmlDecode(matches[0].Groups[2].Value).Trim();
                if(value.Length==0||value.Length>8192||value.IndexOf('<')>=0||value.IndexOf('\0')>=0)return null;
                return Normalize(value);
            }
            catch(RegexMatchTimeoutException){return null;}
        }
        private static string Normalize(string value)=>string.Join(" ",value.Split((char[])null,StringSplitOptions.RemoveEmptyEntries));
        private static string Hash(byte[] bytes)
        {using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").ToLowerInvariant();}
        private static byte[] ReadBounded(string path,int maximum)
        {
            using(var stream=File.OpenRead(path))
            {
                if(stream.Length<=0||stream.Length>maximum)throw new IOException("Invalid evidence size");
                var bytes=new byte[(int)stream.Length];
                int offset=0;
                while(offset<bytes.Length)
                {int n=stream.Read(bytes,offset,bytes.Length-offset);if(n<=0)throw new IOException();offset+=n;}
                if(stream.ReadByte()!=-1)throw new IOException();
                return bytes;
            }
        }
        public static State Run(string evidenceRoot,string resultRoot,VitqraChallengeDirectory.Item item)
        {
            if(item==null||!item.educational||item.webCandidate||!Supports(item.id))return State.Unsupported;
            var review=VitqraEvidenceReview.Inspect(evidenceRoot,item);
            if(review.state!=VitqraEvidenceReview.State.DocumentChecksPassed||
                review.sample!=VitqraEvidenceReview.SampleState.FormatMatches)return State.EvidenceRequired;
            try
            {
                string folder=VitqraEvidence.Folder(evidenceRoot,item.id,item.url);
                var manifest=VitqraEvidence.ReadVerified(folder,item);
                if(manifest==null)return State.EvidenceRequired;
                var raw=ReadBounded(Path.Combine(folder,"source.html.txt"),VitqraEvidence.MaxHtml);
                var input=ReadBounded(Path.Combine(folder,"public-sample.txt"),8192);
                string sourceHash=Hash(raw),inputHash=Hash(input);
                if(!manifest.assets.Any(a=>a.name=="source.html.txt"&&a.sha256==sourceHash)||
                    !manifest.assets.Any(a=>a.name=="public-sample.txt"&&a.sha256==inputHash))return State.EvidenceRequired;
                var utf8=new UTF8Encoding(false,true);
                string html=utf8.GetString(raw),sample=utf8.GetString(input);
                if(!string.Equals(VitqraEvidence.SampleDataset(html),sample,StringComparison.Ordinal))return State.EvidenceRequired;
                string expected=ExpectedOutput(html);
                if(expected==null)return State.OutputUnavailable;
                string result=Solve(item.id,sample);
                if(result==null||Normalize(result)!=expected)return State.Mismatch;
                string expectedHash=Hash(utf8.GetBytes(expected)),resultHash=Hash(utf8.GetBytes(result));
                string target=VitqraEvidence.Folder(resultRoot,item.id,item.url);
                Directory.CreateDirectory(target);
                string path=Path.Combine(target,"sample-check.json");
                if(File.Exists(path))
                {
                    var previous=JsonUtility.FromJson<Checkpoint>(utf8.GetString(ReadBounded(path,32768)));
                    // Recompute every time; a previous status alone is not trusted.
                    if(previous!=null&&previous.version==1&&previous.scope=="public_educational_sample_only"&&
                        previous.challengeId==item.id&&previous.sourceUrl==item.url&&previous.sourceSha256==sourceHash&&
                        previous.inputSha256==inputHash&&previous.expectedSha256==expectedHash&&
                        previous.resultSha256==resultHash&&previous.result==result)return State.Checked;
                    // Preserve an inconsistent checkpoint for inspection.
                    return State.StorageFailure;
                }
                var checkpoint=new Checkpoint {challengeId=item.id,sourceUrl=item.url,sourceSha256=sourceHash,
                    inputSha256=inputHash,expectedSha256=expectedHash,resultSha256=resultHash,result=result,
                    checkedUtc=DateTime.UtcNow.ToString("o",CultureInfo.InvariantCulture)};
                string temp=path+".tmp";
                if(File.Exists(temp))return State.StorageFailure;
                try
                {
                    File.WriteAllText(temp,JsonUtility.ToJson(checkpoint,true),utf8);
                    File.Move(temp,path);
                }
                finally{if(File.Exists(temp))File.Delete(temp);}
                return State.Checked;
            }
            catch(Exception){return State.StorageFailure;}
        }
    }
}
