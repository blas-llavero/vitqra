using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Vitqra
{
    public static class VitqraMultimodalChecks
    {
        [MenuItem("VITQRA/Run Synthetic Multimodal Checks")]
        public static void Run()
        {
            var r=VitqraMultimodal.Run();var again=VitqraMultimodal.Run();
            Check(r.developmentAssigned==400&&r.holdoutAssigned==200&&r.perturbations==600,"Split and cutoff perturbations");
            Check(r.models.Length==5&&r.models.All(m=>m.holdout.Total==88&&m.development.Total==188),"Paired denominators");
            Check(r.developmentUnknown==28&&r.developmentMissing==184&&r.holdoutUnknown==12&&r.holdoutMissing==100,"Exclusions");
            Check(r.models[0].threshold==-2&&r.models[0].holdout.TP==28&&r.models[0].holdout.FP==27&&r.models[0].holdout.TN==24&&r.models[0].holdout.FN==9,"Temporal reference");
            Check(r.models[1].threshold==0&&r.models[1].holdout.TP==25&&r.models[1].holdout.FP==24&&r.models[1].holdout.TN==27&&r.models[1].holdout.FN==12,"ImageProxy reference");
            Check(r.models[2].threshold==0&&r.models[2].holdout.TP==24&&r.models[2].holdout.FP==21&&r.models[2].holdout.TN==30&&r.models[2].holdout.FN==13,"SymptomProxy reference");
            Check(r.models[3].threshold==2&&r.models[3].holdout.TP==13&&r.models[3].holdout.FP==17&&r.models[3].holdout.TN==34&&r.models[3].holdout.FN==24,"GeneticProxy reference");
            Check(r.models[4].threshold==0&&r.models[4].holdout.TP==23&&r.models[4].holdout.FP==19&&r.models[4].holdout.TN==32&&r.models[4].holdout.FN==14,"Fusion reference");
            Check(r.cohortDigest.Length==64&&r.cohortDigest==again.cohortDigest,"Digest stability");
            var ci=VitqraMultimodal.Metrics.Wilson(5,10);
            Check(ci!=null&&ci[0]<.5&&ci[1]>.5&&VitqraMultimodal.Metrics.Wilson(0,0)==null,"Uncertainty");
            Check(r.baseline.TP==0&&r.baseline.FP==0&&r.baseline.Total==r.models[0].holdout.Total,"Baseline");
            Check(!r.ClinicalRelease&&!VitqraLongitudinal.ClinicalRelease&&!new VitqraEvidenceReview.Report().CanStartAutonomousResearch,"Release blocked");
            Debug.Log("VITQRA synthetic multimodal checks passed.");
        }
        private static void Check(bool ok,string label){if(!ok)throw new InvalidOperationException(label);}
    }
}
