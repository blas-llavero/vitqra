using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace Vitqra
{
    // Closed invented educational fixture; no downloads, inference or patient data.
    public static class VitqraEvidenceChainChecks
    {
        [MenuItem("VITQRA/Run Synthetic Evidence Chain Checks")]
        public static void Run()
        {
            var item=VitqraChallengeDirectory.Educational().Single(i=>i.id=="rosalind-dna");
            string root=Path.Combine(Path.GetTempPath(),"vitqra-chain-"+Guid.NewGuid().ToString("N"));
            string evidence=Path.Combine(root,"evidence"),results=Path.Combine(root,"results"),
                reports=Path.Combine(root,"reports"),drafts=Path.Combine(root,"drafts");
            string folder=VitqraEvidence.Folder(evidence,item.id,item.url);
            try
            {
                Check(VitqraResearchPackage.Build(evidence,results,reports,drafts,item,"en")==null,"Missing evidence");
                Directory.CreateDirectory(folder);
                string source="<h2>Sample Dataset</h2><pre>ACGT</pre><h2>Sample Output</h2><pre>1 1 1 1</pre>";
                var manifest=new VitqraEvidence.Manifest {
                    challengeId=item.id,title=item.title,sourceUrl=item.url,
                    retrievedUtc=DateTime.UtcNow.ToString("o",CultureInfo.InvariantCulture),educational=true,
                    repositoryCandidates=new string[0],
                    assets=new[]{Asset(folder,item,"source.html.txt",source,"original_html_bytes"),
                        Asset(folder,item,"source-preview.txt",VitqraWebReader.Extract(source).text,"derived_text_preview"),
                        Asset(folder,item,"public-sample.txt","ACGT","educational_sample")}
                };
                File.WriteAllText(Path.Combine(folder,"manifest.json"),JsonUtility.ToJson(manifest),new UTF8Encoding(false,true));
                Check(VitqraEvidenceReview.Inspect(evidence,item).state==VitqraEvidenceReview.State.DocumentChecksPassed,"Document gate");
                string before=VitqraReproducibility.Build(evidence,results,drafts,item,"en");
                Check(before!=null&&before.Contains("sample_status=not_recorded\n"),"No invented sample");
                Check(VitqraSampleSolver.Run(evidence,results,item)==VitqraSampleSolver.State.Checked,"Sample");
                Check(VitqraStrings.Languages.Length==10&&VitqraStrings.Languages.Distinct().Count()==10,"Locales");
                foreach(string locale in VitqraStrings.Languages)
                {
                    Check(VitqraResearchReport.Save(evidence,results,reports,item,locale),"Save "+locale);
                    var c=VitqraResearchPackage.Build(evidence,results,reports,drafts,item,locale);
                    Check(c!=null&&c.identity.Length==64,"Content "+locale);
                    Check(c.body.Contains("1 1 1 1")&&c.reproducibility.Contains("sample_status=locally_rechecked\n"),"Sample "+locale);
                    Check(c.reproducibility.Contains("draft_status=not_recorded\n")&&
                        c.reproducibility.Contains("clinical_release=blocked\n")&&
                        c.reproducibility.Contains("autonomous_validated_research=blocked\n"),"Scope "+locale);
                    Check(c.html.Contains("lang=\""+locale+"\"")&&c.html.Contains(locale=="ar"?"dir=\"rtl\"":"dir=\"ltr\""),"HTML locale "+locale);
                    Check(VitqraReviewDossier.Matches(c.reviewRequest,c.body,c.html,c.reproducibility),"Review request "+locale);
                    Check(c.reviewRequest.Contains("signature=absent\n")&&c.reviewRequest.Contains("release_for_clinical_use=blocked\n"),"Release "+locale);
                    Check(!VitqraReviewDossier.Matches(c.reviewRequest,c.body+"changed",c.html,c.reproducibility),"Changed report "+locale);
                    var repeat=VitqraResearchPackage.Build(evidence,results,reports,drafts,item,locale);
                    Check(repeat!=null&&repeat.identity==c.identity,"Stable content "+locale);
                }
                string checkpoint=Path.Combine(VitqraEvidence.Folder(results,item.id,item.url),"sample-check.json");
                byte[] original=File.ReadAllBytes(checkpoint);
                File.WriteAllText(checkpoint,"{}",new UTF8Encoding(false,true));
                Check(VitqraResearchPackage.Build(evidence,results,reports,drafts,item,"en")==null,"Changed checkpoint");
                File.WriteAllBytes(checkpoint,original);
                Check(VitqraResearchPackage.Build(evidence,results,reports,drafts,item,"en")!=null,"Restored checkpoint");
                File.WriteAllText(Path.Combine(folder,"source.html.txt"),"changed source");
                Check(VitqraReproducibility.Build(evidence,results,drafts,item,"en")==null&&
                    VitqraResearchPackage.Build(evidence,results,reports,drafts,item,"en")==null,"Changed source");
                Check(!new VitqraEvidenceReview.Report().CanStartAutonomousResearch,"No scientific authority");
                Debug.Log("VITQRA synthetic evidence chain checks passed.");
            }
            finally{if(Directory.Exists(root))Directory.Delete(root,true);}
        }
        private static VitqraEvidence.Asset Asset(string folder,VitqraChallengeDirectory.Item item,string name,string value,string kind)
        {
            byte[] bytes=new UTF8Encoding(false,true).GetBytes(value);
            File.WriteAllBytes(Path.Combine(folder,name),bytes);
            return new VitqraEvidence.Asset{name=name,sourceUrl=item.url,kind=kind,bytes=bytes.Length,
                sha256=VitqraReproducibility.Hash(value),rights="source_terms_apply; redistribution_not_approved"};
        }
        private static void Check(bool ok,string label){if(!ok)throw new InvalidOperationException("Evidence chain: "+label);}
    }
}
