using System;
using UnityEditor;
using UnityEngine;

namespace Vitqra
{
    public static class VitqraResearchDraftChecks
    {
        [MenuItem("VITQRA/Run Research Draft Checks")]
        public static void Run()
        {
            string first=VitqraResearchDraft.Instruction("en",false);
            string second=VitqraResearchDraft.Instruction("en",true);
            Check(first.Contains("two distinct")&&first.Contains("untrusted data"),"Alternatives");
            Check(second.Contains("risks to people")&&second.Contains("equity"),"Human impact");
            Check(second.Contains("Do not claim it wins")&&second.Contains("independent clinical review"),"No unsupported success or clinical advice");
            Check(!new VitqraEvidenceReview.Report().CanStartAutonomousResearch,"Release gate stays closed");
            Check(VitqraResearchDraft.Load("missing-fixture","missing-fixture",null,"en")==null,"Missing evidence");
            Debug.Log("VITQRA research draft checks passed.");
        }
        private static void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);}
    }
}
