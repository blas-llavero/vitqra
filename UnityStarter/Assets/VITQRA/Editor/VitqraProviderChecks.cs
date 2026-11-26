using System;
using UnityEditor;
using UnityEngine;
namespace Vitqra
{
    public static class VitqraProviderChecks
    {
        [MenuItem("VITQRA/Run Provider Settings Checks")]
        public static void Run()
        {
            var s=new VitqraSettings();
            Check(s.LocalAiSelected,"Local default");
            Check(s.CanStartResearch(true,true,false,0),"Local cost gate");
            s.SelectAi("chatgpt");
            Check(!s.LocalAiSelected&&!s.CanStartResearch(true,true,false,0),"Pending provider blocks local research");
            Check(!s.CanStartResearch(true,false,false,0),"Remote gate");
            s.SelectAi("unknown");
            Check(s.LocalAiSelected,"Unknown preference normalizes to local");
            Check(!s.CanStartResearch(true,true,true,1),"Paid requests blocked");
            foreach(string locale in VitqraStrings.Languages)
                foreach(VitqraProviderStrings.Key key in Enum.GetValues(typeof(VitqraProviderStrings.Key)))
                    Check(!string.IsNullOrWhiteSpace(VitqraProviderStrings.Get(locale,key)),"Translation");
            Debug.Log("VITQRA provider settings checks passed.");
        }
        private static void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);}
    }
}
