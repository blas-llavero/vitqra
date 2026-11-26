using System;
using UnityEditor;
using UnityEngine;
namespace Vitqra
{
    public static class VitqraMotifBenchmarkChecks
    {
        [MenuItem("VITQRA/Run Motif Optimization Checks")]
        public static void Run()
        {
            Check(VitqraMotifBenchmark.Search("AAAAA","AAA",true).output=="1 2 3","Overlap");
            Check(VitqraMotifBenchmark.Search("ACGT","AA",true).output=="","No match");
            Check(VitqraMotifBenchmark.Search("ACGT","ACGT",true).output=="1","Full match");
            Check(VitqraMotifBenchmark.Search("ACGT","",true)==null,"Empty rejected");
            Check(VitqraMotifBenchmark.Search("AXGT","A",false)==null,"Alphabet");
            Check(VitqraMotifBenchmark.Search("A","AA",true)==null,"Long motif");
            var trial=VitqraMotifBenchmark.Evaluate("GATATATGCATATACTT","ATAT");
            Check(trial.passed&&trial.cases==71,"Fixed suite");
            Check(trial.kmpComparisons<trial.baselineComparisons&&trial.preferred=="KMP","Comparison count");
            Check(VitqraMotifBenchmark.Load("missing","missing",null)==null,"No invented result");
            Check(!new VitqraEvidenceReview.Report().CanStartAutonomousResearch,"Scientific gate closed");
            Debug.Log("VITQRA motif optimization checks passed.");
        }
        private static void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);}
    }
}
