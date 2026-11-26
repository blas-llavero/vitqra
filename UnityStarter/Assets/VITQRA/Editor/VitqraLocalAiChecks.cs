using System;
using UnityEditor;
using UnityEngine;
namespace Vitqra
{
    public static class VitqraLocalAiChecks
    {
        [MenuItem("VITQRA/Run Local AI Checks")]
        public static void Run()
        {
            Check(VitqraLocalAi.ValidHash(new string('a',64)),"Valid hash format");
            Check(!VitqraLocalAi.ValidHash(new string('z',64)),"Non-hex rejected");
            Check(!VitqraLocalAi.ValidHash(""),"Missing hash rejected");
            Check(VitqraLocalAi.SystemPrompt("ca").Contains("Catalan"),"Catalan");
            Check(VitqraLocalAi.SystemPrompt("ja").Contains("Japanese"),"Japanese");
            Check(VitqraLocalAi.SystemPrompt("it").Contains("English"),"Fallback");
            var go=new GameObject("Local AI test");
            try{Check(!go.AddComponent<VitqraLocalAi>().Configured,"Disabled by default");}
            finally{UnityEngine.Object.DestroyImmediate(go);}
            Debug.Log("VITQRA local AI checks passed.");
        }
        private static void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);}
    }
}
