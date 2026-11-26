using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Vitqra
{
    public static class VitqraResearchReportChecks
    {
        [MenuItem("VITQRA/Run Research Report Checks")]
        public static void Run()
        {
            var r=new VitqraResearchReport.Report {title="Fixture",language="ar"};
            r.sections.Add(new VitqraResearchReport.Section {heading="Test",body="<script>bad()</script> & text"});
            string html=VitqraResearchReport.Html(r);
            Check(html.Contains("dir=\"rtl\"")&&html.Contains("&lt;script&gt;")&&!html.Contains("<script>"),"Escaping and direction");
            var item=VitqraChallengeDirectory.Educational()[0];
            string root=Path.Combine(Path.GetTempPath(),"vitqra-report-"+Guid.NewGuid().ToString("N"));
            string evidence=Path.Combine(root,"evidence"),results=Path.Combine(root,"results"),reports=Path.Combine(root,"reports");
            try
            {
                Check(VitqraResearchReport.Build(evidence,results,item,"en")==null,"No evidence");
                string folder=VitqraEvidence.Folder(evidence,item.id,item.url);
                Directory.CreateDirectory(folder);
                string source="<h2>Sample Dataset</h2><pre>ACGT</pre><h2>Sample Output</h2><pre>1 1 1 1</pre>";
                var manifest=new VitqraEvidence.Manifest {challengeId=item.id,title=item.title,sourceUrl=item.url,
                    retrievedUtc=DateTime.UtcNow.ToString("o"),educational=true,repositoryCandidates=new string[0],
                    assets=new[]{Asset(folder,item,"source.html.txt",source,"original_html_bytes"),
                        Asset(folder,item,"source-preview.txt",VitqraWebReader.Extract(source).text,"derived_text_preview"),
                        Asset(folder,item,"public-sample.txt","ACGT","educational_sample")}};
                File.WriteAllText(Path.Combine(folder,"manifest.json"),JsonUtility.ToJson(manifest));
                var before=VitqraResearchReport.Build(evidence,results,item,"en");
                Check(before!=null&&VitqraResearchReport.PlainText(before).Contains("No validated sample result"),"No fabricated result");
                Check(VitqraSampleSolver.Run(evidence,results,item)==VitqraSampleSolver.State.Checked,"Synthetic sample verified");
                Check(VitqraResearchReport.Save(evidence,results,reports,item,"ca"),"Save localized report");
                string text=VitqraResearchReport.Load(evidence,results,reports,item,"ca");
                Check(text!=null&&text.Contains("1 1 1 1")&&text.Contains("Només mostra educativa"),"Actual sample and scope");
                Check(VitqraResearchReport.Save(evidence,results,reports,item,"ca"),"Reuse identical report");
                string saved=Path.Combine(VitqraEvidence.Folder(reports,item.id,item.url),"ca","report.txt");
                File.WriteAllText(saved,"tampered");
                Check(VitqraResearchReport.Load(evidence,results,reports,item,"ca")==null,"Reject modified report");
                Check(!VitqraResearchReport.Save(evidence,results,reports,item,"ca"),"Preserve modified report");
                Check(!new VitqraEvidenceReview.Report().CanStartAutonomousResearch,"Scientific gate closed");
            }
            finally{if(Directory.Exists(root))Directory.Delete(root,true);}
            Debug.Log("VITQRA research report checks passed.");
        }
        private static VitqraEvidence.Asset Asset(string folder,VitqraChallengeDirectory.Item item,string name,string value,string kind)
        {
            byte[] bytes=Encoding.UTF8.GetBytes(value);
            File.WriteAllBytes(Path.Combine(folder,name),bytes);
            using(var sha=SHA256.Create())return new VitqraEvidence.Asset {name=name,sourceUrl=item.url,kind=kind,
                bytes=bytes.Length,rights="source_terms_apply; redistribution_not_approved",
                sha256=BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").ToLowerInvariant()};
        }
        private static void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);}
    }
}
