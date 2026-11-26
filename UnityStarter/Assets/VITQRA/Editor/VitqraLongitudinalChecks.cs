using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Vitqra
{
    public static class VitqraLongitudinalChecks
    {
        [MenuItem("VITQRA/Run Synthetic Longitudinal Checks")]
        public static void Run()
        {
            Check(VitqraLongitudinal.At(-1,0)==null&&VitqraLongitudinal.At(0,181)==null,"Bounds");
            Check(VitqraLongitudinal.At(0,30).observations.Count==3,"Delayed availability");
            Check(VitqraLongitudinal.At(0,35).observations.Count==4,"Delayed row visible");
            for(int p=0;p<3;p++)for(int day=0;day<=180;day++)
                Check(VitqraLongitudinal.At(p,day).observations.All(o=>o.participant==p&&o.researchAllowed&&o.observedDay<=day&&o.availableDay<=day),"Cutoff");
            var end=VitqraLongitudinal.At(1,180);
            Check(end.observations.Count==13&&end.observations.Any(o=>!o.value.HasValue),"Scope and missingness");
            Check(VitqraLongitudinal.Change(end,VitqraLongitudinal.Feature.A)==36,"Change");
            Check(end.digest==VitqraLongitudinal.At(1,180).digest&&end.digest!=VitqraLongitudinal.At(1,179).digest,"Digest");
            Check(VitqraRetrospective.CutoffFixture(),"Future-data perturbation");
            var r=VitqraRetrospective.Run();var again=VitqraRetrospective.Run();
            Check(r.trainAssigned==80&&r.testAssigned==40,"Split");
            Check(r.train.Total+r.trainMissing+r.trainCensored==80&&r.test.Total+r.testMissing+r.testCensored==40,"Denominators");
            Check(r.threshold==2&&r.train.Total==66&&r.test.Total==35&&r.test.TP==7&&r.test.FP==6&&r.test.TN==15&&r.test.FN==7,"Independent Python reference");
            Check(r.cohortDigest=="f18ee7c934ed0c1b88cbda3c259226c4241ab9b701eb2da47fd86f1c1a8d67b1","Reference cohort hash");
            Check(r.baseline.Total==r.test.Total&&r.baseline.TP==0&&r.baseline.FP==0,"Baseline");
            Check(r.cohortDigest==again.cohortDigest&&r.threshold==again.threshold,"Determinism");
            Check(!r.ClinicalRelease&&!VitqraLongitudinal.ClinicalRelease,"No clinical release");
            Debug.Log("VITQRA synthetic longitudinal checks passed.");
        }
        private static void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);}
    }
}
