using System;
using UnityEditor;
using UnityEngine;
namespace Vitqra
{
    public static class VitqraReviewDossierChecks
    {
        [MenuItem("VITQRA/Run Review Dossier Checks")]
        public static void Run()
        {
            string a=VitqraReviewDossier.Build("report","html","audit");
            Check(a!=null&&a.Contains("unsigned_review_request_not_certificate"),"Unsigned");
            Check(a.Contains("release_for_clinical_use=blocked"),"Blocked");
            Check(VitqraReviewDossier.Matches(a,"report","html","audit"),"Stable");
            Check(!VitqraReviewDossier.Matches(a,"edited","html","audit"),"Text change");
            Check(!VitqraReviewDossier.Matches(a,"report","edited","audit"),"HTML change");
            Check(!VitqraReviewDossier.Matches(a,"report","html","edited"),"Audit change");
            Check(!VitqraReviewDossier.Matches(a.Replace("signature=absent","signature=approved"),"report","html","audit"),"No forged approval");
            Check(!VitqraReviewDossier.Matches(a.Replace("release_for_clinical_use=blocked","release_for_clinical_use=approved"),"report","html","audit"),"No release override");
            Check(VitqraReviewDossier.Build(null,"html","audit")==null,"Missing");
            Check(VitqraReviewDossier.Build("report","html",new string('x',32769))==null,"Bounded");
            Check(!new VitqraEvidenceReview.Report().CanStartAutonomousResearch,"Existing gate");
            Debug.Log("VITQRA review dossier checks passed.");
        }
        private static void Check(bool ok,string label){if(!ok)throw new InvalidOperationException(label);}
    }
}
