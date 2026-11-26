using System;
using UnityEditor;
using UnityEngine;

namespace Vitqra
{
    public static class VitqraEvidenceChecks
    {
        [MenuItem("VITQRA/Run Evidence Checks")]
        public static void Run()
        {
            // Synthetic fixtures, never used as actual research data.
            var item=new VitqraChallengeDirectory.Item {id="fr-2026-12345",title="Fixture",
                url="https://www.federalregister.gov/documents/fixture",publicationDate="2026-01-02",
                officialPdf="https://www.govinfo.gov/content/pkg/FR-2026-01-02/pdf/2026-12345.pdf"};
            Check(VitqraEvidence.ValidPdf(item),"Matching official PDF route");
            item.officialPdf="https://www.govinfo.gov/content/pkg/FR-2026-01-02/pdf/2026-54321.pdf";
            Check(!VitqraEvidence.ValidPdf(item),"Mismatched document");
            item.webCandidate=true;
            Check(!VitqraEvidence.Eligible(item),"Unverified web candidate excluded");
            Check(VitqraEvidence.PdfSignature(new byte[]{37,80,68,70,45}),"PDF signature");
            Check(!VitqraEvidence.PdfSignature(new byte[]{1,2,3}),"Wrong signature");
            Check(VitqraEvidence.SampleDataset("<h2>Sample Dataset</h2><pre>ACGT</pre>")=="ACGT","Sample");
            Check(VitqraEvidence.SampleDataset("<h2>Dataset</h2><pre>ACGT</pre>")==null,"Unlabeled data");
            Check(VitqraEvidence.RepositoryLinks("https://zenodo.org/records/123 https://github.com/example/project").Length==2,"Candidate links");
            Debug.Log("VITQRA evidence checks passed.");
        }
        private static void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);}
    }
}
