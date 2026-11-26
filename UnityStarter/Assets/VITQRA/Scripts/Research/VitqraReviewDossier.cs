using System;
using System.Text;

namespace Vitqra
{
    // An unsigned request for independent review. It is NOT a certificate,
    // signature, authorization, clinical evaluation or regulatory decision.
    // All current research packages are nonclinical and remain blocked.
    public static class VitqraReviewDossier
    {
        public const string Protocol="independent-review-request-v1";
        public static string Build(string reportText,string reportHtml,string audit)
        {
            if(string.IsNullOrEmpty(reportText)||string.IsNullOrEmpty(reportHtml)||string.IsNullOrEmpty(audit))return null;
            var utf8=new UTF8Encoding(false,true);
            try
            {
                if(utf8.GetByteCount(reportText)>262144||utf8.GetByteCount(reportHtml)>262144||utf8.GetByteCount(audit)>32768)return null;
                string record="protocol="+Protocol+"\n"+
                    "document_kind=unsigned_review_request_not_certificate\n"+
                    "scope=nonclinical_research_only\n"+
                    "report_text_sha256="+VitqraReproducibility.Hash(reportText)+"\n"+
                    "report_html_sha256="+VitqraReproducibility.Hash(reportHtml)+"\n"+
                    "reproducibility_sha256="+VitqraReproducibility.Hash(audit)+"\n"+
                    "intended_purpose_review=pending\n"+
                    "scientific_evidence_review=pending\n"+
                    "technical_analytical_validation=pending_independent_review\n"+
                    "external_replication=pending\n"+
                    "clinical_performance=not_established\n"+
                    "benefit_risk_review=pending\n"+
                    "ethics_privacy_and_data_rights_review=pending\n"+
                    "reviewer_identity_and_authority=not_verified\n"+
                    "signature=absent\n"+
                    "regulatory_conformity=not_assessed\n"+
                    "release_for_clinical_use=blocked\n";
                return record+"record_body_sha256="+VitqraReproducibility.Hash(record)+"\n";
            }
            catch(ArgumentException){return null;}
        }
        public static bool Matches(string record,string reportText,string reportHtml,string audit)
        {
            if(string.IsNullOrEmpty(record)||record.Length>4096)return false;
            string expected=Build(reportText,reportHtml,audit);
            return expected!=null&&string.Equals(record,expected,StringComparison.Ordinal);
        }
    }
}
