using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;
using R=Vitqra.VitqraReportStrings.Key;

namespace Vitqra
{
    // Local report only: no inference, scripts, networking or execution of source material.
    public static class VitqraResearchReport
    {
        private const int MaxFile=65536;
        private const long MaxReports=8L*1024*1024;
        private static readonly UTF8Encoding Utf8=new UTF8Encoding(false,true);
        public sealed class Section
        {
            public string heading,body;
        }
        public sealed class Report
        {
            public string language,title;
            public List<Section> sections=new List<Section>();
        }
        private static string T(string language,R key)=>VitqraReportStrings.Get(language,key);
        private static string Read(string path,int max)
        {
            using(var f=File.OpenRead(path))
            {
                if(f.Length<=0||f.Length>max)throw new IOException("Invalid file size");
                var b=new byte[(int)f.Length];
                int n=0;
                while(n<b.Length){int read=f.Read(b,n,b.Length-n);if(read<=0)throw new IOException();n+=read;}
                if(f.ReadByte()!=-1)throw new IOException();
                return Utf8.GetString(b);
            }
        }
        private static string Folder(string root,VitqraChallengeDirectory.Item item,string locale)=>
            Path.Combine(VitqraEvidence.Folder(root,item.id,item.url),VitqraStrings.ResolveLanguage(locale));
        private static void Add(Report r,R heading,string body)=>r.sections.Add(new Section{heading=T(r.language,heading),body=body});
        private static R? MethodKey(string id)
        {
            switch(id)
            {
                case "rosalind-dna":return R.Dna;
                case "rosalind-rna":return R.Rna;
                case "rosalind-revc":return R.Reverse;
                case "rosalind-hamm":return R.Hamming;
                case "rosalind-subs":return R.Motif;
                default:return null;
            }
        }
        public static string PlainText(Report r)
        {
            var s=new StringBuilder();
            s.AppendLine(r.title);
            foreach(var part in r.sections)s.AppendLine().AppendLine(part.heading).AppendLine(part.body);
            return s.ToString();
        }
        public static string Html(Report r)
        {
            // Only fixed markup and escaped literal text. No links, remote fonts or scripts.
            string E(string value)=>WebUtility.HtmlEncode(value??"");
            var s=new StringBuilder("<!doctype html><html lang=\"").Append(E(r.language)).Append("\" dir=\"")
                .Append(r.language=="ar"?"rtl":"ltr").Append("\"><head><meta charset=\"utf-8\">")
                .Append("<meta name=\"viewport\" content=\"width=device-width,initial-scale=1\">")
                .Append("<meta http-equiv=\"Content-Security-Policy\" content=\"default-src 'none'; style-src 'unsafe-inline'; base-uri 'none'; form-action 'none'\">")
                .Append("<title>").Append(E(r.title)).Append("</title><style>")
                .Append("body{max-width:52rem;margin:2rem auto;padding:0 1.5rem;font:16px/1.55 system-ui,sans-serif;color:#142a38;background:#fff}h1{color:#125761;border-bottom:2px solid #258c8a}h2{font-size:1.15rem;color:#125761}section{margin:1.4rem 0}p{white-space:pre-wrap;overflow-wrap:anywhere}footer{border-top:1px solid #abbcc3;padding-top:1rem;font-size:.9rem}")
                .Append("</style></head><body><header><p>VITQRA — Science without barriers</p><h1>").Append(E(r.title)).Append("</h1></header>");
            foreach(var part in r.sections)s.Append("<section><h2>").Append(E(part.heading)).Append("</h2><p dir=\"auto\">").Append(E(part.body)).Append("</p></section>");
            return s.Append("<footer>VITQRA v0.5.2</footer></body></html>").ToString();
        }
        // Reconstruct from checked local evidence; no pre-existing report is trusted.
        public static Report Build(string evidenceRoot,string resultRoot,VitqraChallengeDirectory.Item item,string language)
        {
            if(item==null||!VitqraEvidence.Eligible(item))return null;
            try
            {
                var review=VitqraEvidenceReview.Inspect(evidenceRoot,item);
                if(review.state!=VitqraEvidenceReview.State.DocumentChecksPassed&&review.state!=VitqraEvidenceReview.State.Partial)return null;
                string folder=VitqraEvidence.Folder(evidenceRoot,item.id,item.url);
                var manifest=VitqraEvidence.ReadVerified(folder,item);
                if(manifest==null)return null;
                var r=new Report {language=VitqraStrings.ResolveLanguage(language)};
                r.title=T(r.language,R.Title);
                string Get(R key)=>T(r.language,key);
                Add(r,R.Scope,Get(item.educational?R.Education:R.Notice)+"\n"+Get(review.state==VitqraEvidenceReview.State.DocumentChecksPassed?R.Passed:R.Partial));
                Add(r,R.Challenge,manifest.title+"\n"+Get(R.Preview)+"\nsource-preview.txt");
                var inventory=new StringBuilder(manifest.sourceUrl).Append("\n").Append(Get(R.Retrieved)).Append(": ").Append(manifest.retrievedUtc)
                    .Append("\n").Append(Get(R.Inventory));
                foreach(var asset in manifest.assets)
                    inventory.Append("\n").Append(asset.name).Append(" | ").Append(asset.bytes.ToString(CultureInfo.InvariantCulture))
                        .Append(" B\n").Append(asset.sha256).Append("\n").Append(asset.sourceUrl);
                Add(r,R.Provenance,inventory.ToString());
                string method=Get(R.DocumentMethod),result=Get(R.NoResult);
                if(item.educational&&VitqraSampleSolver.Supports(item.id)&&review.state==VitqraEvidenceReview.State.DocumentChecksPassed)
                {
                    string checkpoint=Path.Combine(VitqraEvidence.Folder(resultRoot,item.id,item.url),"sample-check.json");
                    // No checkpoint means no claim of an executed sample. Run only rechecks an existing checkpoint.
                    if(File.Exists(checkpoint)&&VitqraSampleSolver.Run(evidenceRoot,resultRoot,item)==VitqraSampleSolver.State.Checked)
                    {
                        string sample=Read(Path.Combine(folder,"public-sample.txt"),8192);
                        string expected=VitqraSampleSolver.ExpectedOutput(Read(Path.Combine(folder,"source.html.txt"),VitqraEvidence.MaxHtml));
                        string actual=VitqraSampleSolver.Solve(item.id,sample);
                        if(actual!=null&&expected!=null&&string.Equals(actual,expected,StringComparison.Ordinal))
                        {
                            method+="\n"+Get(R.SampleMethod)+"\n"+Get(MethodKey(item.id).Value);
                            result=Get(R.Match)+"\n"+Get(R.Computed)+":\n"+actual+"\n"+Get(R.Expected)+":\n"+expected;
                        }
                    }
                }
                Add(r,R.Method,method);
                Add(r,R.Results,result);
                Add(r,R.Limits,Get(R.Warning)+"\n"+Get(R.Rights));
                return r;
            }
            catch(Exception){return null;}
        }
        public static bool Save(string evidenceRoot,string resultRoot,string reportRoot,VitqraChallengeDirectory.Item item,string language)
        {
            var r=Build(evidenceRoot,resultRoot,item,language);
            if(r==null)return false;
            string stage=null;
            try
            {
                string target=Folder(reportRoot,item,r.language);
                string plain=PlainText(r),html=Html(r);
                if(Utf8.GetByteCount(plain)>MaxFile||Utf8.GetByteCount(html)>MaxFile)return false;
                if(Directory.Exists(target))
                    return Read(Path.Combine(target,"report.txt"),MaxFile)==plain&&Read(Path.Combine(target,"report.html"),MaxFile)==html;
                Directory.CreateDirectory(reportRoot);
                long used=0;
                foreach(string f in Directory.EnumerateFiles(reportRoot,"*",SearchOption.AllDirectories))
                {used+=new FileInfo(f).Length;if(used>MaxReports)return false;}
                if(used+Utf8.GetByteCount(plain)+Utf8.GetByteCount(html)>MaxReports)return false;
                Directory.CreateDirectory(Path.GetDirectoryName(target));
                stage=target+".pending-"+Guid.NewGuid().ToString("N");
                Directory.CreateDirectory(stage);
                File.WriteAllText(Path.Combine(stage,"report.txt"),plain,Utf8);
                File.WriteAllText(Path.Combine(stage,"report.html"),html,Utf8);
                Directory.Move(stage,target);
                stage=null;
                return true;
            }
            catch(Exception){return false;}
            finally{if(stage!=null)try{Directory.Delete(stage,true);}catch(Exception){}}
        }
        public static string Load(string evidenceRoot,string resultRoot,string reportRoot,VitqraChallengeDirectory.Item item,string language)
        {
            var r=Build(evidenceRoot,resultRoot,item,language);
            if(r==null)return null;
            try
            {
                string folder=Folder(reportRoot,item,r.language);
                string plain=Read(Path.Combine(folder,"report.txt"),MaxFile);
                string html=Read(Path.Combine(folder,"report.html"),MaxFile);
                return plain==PlainText(r)&&html==Html(r)?plain:null;
            }
            catch(Exception){return null;}
        }
    }
}
