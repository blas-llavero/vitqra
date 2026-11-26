using System;
using UnityEditor;
using UnityEngine;
namespace Vitqra
{
    public static class VitqraReproducibilityChecks
    {
        [MenuItem("VITQRA/Run Reproducibility Checks")]
        public static void Run()
        {
            Check(VitqraReproducibility.Hash("abc")=="ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad","SHA-256");
            Check(VitqraReproducibility.Hash("abc")!=VitqraReproducibility.Hash("abd"),"Changed bytes");
            Check(VitqraReproducibility.EscapeValue("x%\r\n\t")=="x%0025%000D%000A%0009","Escaping");
            Check(VitqraReproducibility.EscapeValue("a\u2028b")=="a%2028b","Unicode separator");
            Check(VitqraReproducibility.Status(false,true)=="not_recorded","Missing");
            Check(VitqraReproducibility.Status(true,false)=="inconsistent_or_unavailable","Mismatch");
            Check(VitqraReproducibility.Status(true,true)=="locally_rechecked","Checked");
            Check(VitqraReproducibility.Build("missing","missing","missing",null,"en")==null,"No invented result");
            foreach(var locale in VitqraStrings.Languages)Check(VitqraReproducibility.Heading(locale).Length>0,"Locale");
            var review=new VitqraEvidenceReview.Report();
            Check(!review.CanStartAutonomousResearch&&!review.scientificValidationComplete&&!review.rightsVerified,"Gates closed");
            Debug.Log("VITQRA reproducibility checks passed.");
        }
        private static void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);}
    }
}
