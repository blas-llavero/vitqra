using System;
using UnityEditor;
using UnityEngine;

namespace Vitqra
{
    public static class VitqraResearchModelSelectorChecks
    {
        private static VitqraChatAccount.Model M(string slug,string name="Fixture")=>
            new VitqraChatAccount.Model {slug=slug,name=name};
        [MenuItem("VITQRA/Run Research Model Selection Checks")]
        public static void Run()
        {
            var a=new[]{M("gpt-6-luna"),M("gpt-6.2-sol"),M("gpt-6.2-astra")};
            Check(VitqraResearchModelSelector.Resolve(a,"")=="gpt-6.2-astra","Quality over server order");
            Array.Reverse(a);
            Check(VitqraResearchModelSelector.Resolve(a,"")=="gpt-6.2-astra","Order independent");
            Check(VitqraResearchModelSelector.Resolve(new[]{M("gpt-6-luna"),M("gpt-6.2-sol")},"")=="gpt-6.2-sol","Best available");
            Check(VitqraResearchModelSelector.Resolve(new[]{M("gpt-6-luna"),M("gpt-6.1-terra")},"")=="gpt-6.1-terra","Known fallback");
            Check(VitqraResearchModelSelector.Resolve(a,"gpt-6-luna")=="gpt-6-luna","Manual override");
            Check(VitqraResearchModelSelector.Resolve(a,"missing-model")=="","No silent substitution");
            Check(VitqraResearchModelSelector.Resolve(new[]{M("unknown","GPT-6.2 Astra"),M("gpt-image-2.6-sunburst")},"")=="","No guessed ranking");
            Check(VitqraResearchModelSelector.Resolve(new[]{M("unknown")},"unknown")=="unknown","Unknown manual choice");
            Check(VitqraResearchModelSelector.Resolve(Array.Empty<VitqraChatAccount.Model>(),"")=="","Empty catalog");
            Check(VitqraResearchModelSelector.Resolve(null,"")=="","Missing catalog");
            Check(VitqraResearchModelSelector.Resolve(new[]{null,M("gpt-6.2-sol"),M("gpt-6.2-sol")},"")=="gpt-6.2-sol","Nulls and duplicate IDs");
            Debug.Log("VITQRA research model selection checks passed.");
        }
        private static void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);}
    }
}
