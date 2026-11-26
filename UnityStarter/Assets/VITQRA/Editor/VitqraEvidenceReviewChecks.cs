using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Vitqra
{
    public static class VitqraEvidenceReviewChecks
    {
        [MenuItem("VITQRA/Run Evidence Review Checks")]
        public static void Run()
        {
            var item=VitqraChallengeDirectory.Educational()[0];
            Check(VitqraEvidenceReview.SourceIdentityMatches(item),"Curated identity");
            var notice=new VitqraChallengeDirectory.Item {id="fr-2026-12345",publicationDate="2026-01-02",
                url="https://www.federalregister.gov/documents/2026/01/02/2026-12345/fixture"};
            Check(VitqraEvidenceReview.SourceIdentityMatches(notice),"Notice route");
            notice.publicationDate="2026-01-03";
            Check(!VitqraEvidenceReview.SourceIdentityMatches(notice),"Wrong publication date");
            Check(VitqraEvidenceReview.CheckSample("rosalind-dna","ACGT")==VitqraEvidenceReview.SampleState.FormatMatches,"DNA sample");
            Check(VitqraEvidenceReview.CheckSample("rosalind-dna","AX")==VitqraEvidenceReview.SampleState.FormatMismatch,"Invalid alphabet");
            Check(VitqraEvidenceReview.CheckSample("rosalind-hamm","ACGT\nAC")==VitqraEvidenceReview.SampleState.FormatMismatch,"Unequal lengths");
            Check(VitqraEvidenceReview.CheckSample("rosalind-gc","example")==VitqraEvidenceReview.SampleState.NotAssessed,"No invented assessment");
            string root=Path.Combine(Path.GetTempPath(),"vitqra-review-"+Guid.NewGuid().ToString("N"));
            try
            {
                Check(VitqraEvidenceReview.Inspect(root,item).state==VitqraEvidenceReview.State.Missing,"Missing snapshot");
                string folder=VitqraEvidence.Folder(root,item.id,item.url);
                Directory.CreateDirectory(folder);
                // Synthetic HTML only; no external page or real research data.
                string html="<h2>Sample Dataset</h2><pre>ACGT</pre>";
                var manifest=new VitqraEvidence.Manifest {challengeId=item.id,title=item.title,sourceUrl=item.url,
                    retrievedUtc=DateTime.UtcNow.ToString("o"),educational=true,repositoryCandidates=new string[0],
                    assets=new[]{Write(folder,item,"source.html.txt",html,"original_html_bytes"),
                        Write(folder,item,"source-preview.txt",VitqraWebReader.Extract(html).text,"derived_text_preview"),
                        Write(folder,item,"public-sample.txt","ACGT","educational_sample")}};
                Save(folder,manifest);
                var result=VitqraEvidenceReview.Inspect(root,item);
                Check(result.state==VitqraEvidenceReview.State.DocumentChecksPassed,"Consistent snapshot");
                Check(!result.CanStartAutonomousResearch&&!result.scientificValidationComplete,"No false research readiness");
                // Even if a modified derived file has a matching manifest hash,
                // recomputation from the original HTML must reject it.
                manifest.assets[1]=Write(folder,item,"source-preview.txt","Changed text","derived_text_preview");
                Save(folder,manifest);
                Check(VitqraEvidenceReview.Inspect(root,item).state==VitqraEvidenceReview.State.Inconsistent,"Derived text mismatch");
                File.WriteAllText(Path.Combine(folder,"source.html.txt"),"Changed source");
                Check(VitqraEvidenceReview.Inspect(root,item).state==VitqraEvidenceReview.State.Inconsistent,"Hash mismatch");
            }
            finally{if(Directory.Exists(root))Directory.Delete(root,true);}
            Debug.Log("VITQRA evidence review checks passed.");
        }
        private static VitqraEvidence.Asset Write(string folder,VitqraChallengeDirectory.Item item,string name,string content,string kind)
        {
            byte[] bytes=Encoding.UTF8.GetBytes(content);
            File.WriteAllBytes(Path.Combine(folder,name),bytes);
            using(var sha=SHA256.Create())return new VitqraEvidence.Asset {name=name,sourceUrl=item.url,kind=kind,
                bytes=bytes.Length,rights="source_terms_apply; redistribution_not_approved",
                sha256=BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").ToLowerInvariant()};
        }
        private static void Save(string folder,VitqraEvidence.Manifest m)=>File.WriteAllText(Path.Combine(folder,"manifest.json"),JsonUtility.ToJson(m));
        private static void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);}
    }
}
