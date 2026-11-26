using System;
using UnityEditor;
using UnityEngine;
using Q=Vitqra.VitqraDataQuality;

namespace Vitqra
{
    public static class VitqraDataQualityChecks
    {
        [MenuItem("VITQRA/Run Synthetic Data Quality Checks")]
        public static void Run()
        {
            CheckCase(Q.Scenario.Regular,Q.Gate.ReadyForReview,30,0,0);
            CheckCase(Q.Scenario.DifficultDaysOnly,Q.Gate.InsufficientCoverage,0,7,30);
            CheckCase(Q.Scenario.Gap,Q.Gate.InsufficientCoverage,24,0,6);
            CheckCase(Q.Scenario.LateArrival,Q.Gate.InsufficientCoverage,29,0,1);
            CheckCase(Q.Scenario.Duplicate,Q.Gate.InvalidData,29,0,1);
            CheckCase(Q.Scenario.MissingValue,Q.Gate.InsufficientCoverage,29,0,1);
            CheckCase(Q.Scenario.StableOnly,Q.Gate.ReadyForReview,30,0,0);
            CheckCase(Q.Scenario.RegularWithExtras,Q.Gate.ReadyForReview,30,7,0);
            CheckCase(Q.Scenario.InvalidValue,Q.Gate.InvalidData,29,0,1);
            int count=0;
            foreach(Q.Scenario s in Enum.GetValues(typeof(Q.Scenario)))for(int day=1;day<=31;day++)
            {
                var r=Q.Evaluate(s,day);
                Check(r.observed+r.missing==r.expected&&r.low+r.high==r.observed,"Denominators");
                Check(r.gate!=Q.Gate.ReadyForReview||(day>=30&&r.observed==30&&r.duplicates==0&&r.invalid==0),"Coverage gate");
                Check(!r.CanUseForResearch&&!r.ClinicalRelease,"Release blocked");
                count++;
            }
            Check(count==279,"Cutoff cases");
            Check(Q.Evaluate(Q.Scenario.LateArrival,31).gate==Q.Gate.ReadyForReview,"Late arrival");
            Check(Q.Evaluate(Q.Scenario.StableOnly,30).meanTimes100==0,"Zero is not missing");
            Check(!Q.Evaluate(Q.Scenario.DifficultDaysOnly,30).meanTimes100.HasValue,"No scheduled denominator");
            Check(Q.Evaluate(Q.Scenario.Regular,29).gate==Q.Gate.AwaitingWindow,"Incomplete window");
            Check(Q.Evaluate(Q.Scenario.Regular,30).digest.Length==64&&Q.Evaluate(Q.Scenario.Regular,30).digest==Q.Evaluate(Q.Scenario.Regular,30).digest,"Determinism");
            Check(Q.Evaluate(Q.Scenario.Regular,30).digest!=Q.Evaluate(Q.Scenario.RegularWithExtras,30).digest,"Extra observations bound");
            Check(Q.Evaluate((Q.Scenario)999,30)==null&&Q.Evaluate(Q.Scenario.Regular,0)==null,"Invalid request");
            Debug.Log("VITQRA synthetic data quality checks passed.");
        }
        private static void CheckCase(Q.Scenario s,Q.Gate g,int n,int e,int gap)
        {
            var r=Q.Evaluate(s,30);
            Check(r.gate==g&&r.observed==n&&r.extras==e&&r.longestGap==gap,s.ToString());
        }
        private static void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);}
    }
}
