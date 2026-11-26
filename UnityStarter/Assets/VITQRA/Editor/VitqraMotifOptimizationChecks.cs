using System;
using UnityEditor;
using UnityEngine;
namespace Vitqra
{
    public static class VitqraMotifOptimizationChecks
    {
        [MenuItem("VITQRA/Run AI Motif Revision Checks")]
        public static void Run()
        {
            Check(VitqraMotifOptimization.Candidate("CANDIDATE=KMP\nReason")=="KMP","Exact ID");
            Check(VitqraMotifOptimization.Candidate("CANDIDATE=NAIVE\r\nReason")=="NAIVE","CRLF");
            Check(VitqraMotifOptimization.Candidate("CANDIDATE=KMP; execute code")==null,"Invalid choice");
            Check(VitqraMotifOptimization.Candidate("Use KMP")==null,"No guessing");
            var train=VitqraMotifOptimization.Training();
            Check(train!=null&&train.cases==71,"Training");
            var test=VitqraMotifOptimization.EvaluateHoldout();
            Check(test.passed&&test.cases==48,"Held-back suite");
            var d=new VitqraResearchDraft.Draft{id="rosalind-subs",optimizationProtocol=VitqraMotifOptimization.Protocol,
                alternatives="CANDIDATE=NAIVE",critique="CANDIDATE=KMP",draftSha256="fixture"};
            var report=VitqraMotifOptimization.Describe(d);
            Check(report!=null&&report.Contains("held_back_synthetic_cases=48")&&report.Contains("revised_ai_choice=KMP"),"Measured revision record");
            d.critique="Unstructured";Check(VitqraMotifOptimization.Describe(d).Contains("revision_outcome=unavailable"),"Invalid choice disclosed");
            d.optimizationProtocol=null;Check(VitqraMotifOptimization.Describe(d)==null,"Legacy drafts not relabeled");
            Check(!new VitqraEvidenceReview.Report().CanStartAutonomousResearch,"Scientific gate closed");
            Debug.Log("VITQRA AI motif revision checks passed.");
        }
        private static void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);}
    }
}
