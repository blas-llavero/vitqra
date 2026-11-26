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
            Check(VitqraResearchModelSelector.Resolve(a,"",Array.Empty<string>())=="gpt-6.2-astra","Quality over server order");
            Array.Reverse(a);
            Check(VitqraResearchModelSelector.Resolve(a,"",Array.Empty<string>())=="gpt-6.2-astra","Order independent");
            Check(VitqraResearchModelSelector.Resolve(new[]{M("gpt-6-luna"),M("gpt-6.2-sol")},"",Array.Empty<string>())=="gpt-6.2-sol","Best available");
            Check(VitqraResearchModelSelector.Resolve(new[]{M("gpt-6-luna"),M("gpt-6.1-terra")},"",Array.Empty<string>())=="gpt-6.1-terra","Known fallback");
            Check(VitqraResearchModelSelector.Resolve(a,"gpt-6-luna")=="gpt-6-luna","Manual override");
            Check(VitqraResearchModelSelector.Resolve(a,"missing-model")=="","No silent substitution");
            Check(VitqraResearchModelSelector.Resolve(new[]{M("unknown","GPT-6.2 Astra"),M("gpt-image-2.6-sunburst")},"",Array.Empty<string>())=="","No guessed ranking");
            Check(VitqraResearchModelSelector.Resolve(new[]{M("unknown")},"unknown")=="unknown","Unknown manual choice");
            Check(VitqraResearchModelSelector.Resolve(Array.Empty<VitqraChatAccount.Model>(),"")=="","Empty catalog");
            Check(VitqraResearchModelSelector.Resolve(null,"")=="","Missing catalog");
            Check(VitqraResearchModelSelector.Resolve(new[]{null,M("gpt-6.2-sol"),M("gpt-6.2-sol")},"")=="gpt-6.2-sol","Nulls and duplicate IDs");
            // Synthetic future display names, not claims that these models exist.
            var future=new[]{M("fixture-new","GPT-99.1 Example"),M("gpt-6.2-astra","GPT-6.2 Astra")};
            var official=VitqraResearchModelGuidance.Parse("| Research decisions | GPT-99.1 Example, GPT-6.2 Astra |");
            Check(official.Length==2,"Strict research row");
            Check(VitqraResearchModelSelector.Resolve(future,"",official)=="fixture-new","New official ranking wins");
            Check(VitqraResearchModelSelector.Resolve(future,"gpt-6.2-astra",official)=="gpt-6.2-astra","Manual preserved");
            Check(VitqraResearchModelSelector.Resolve(new[]{M("gpt-6.2-astra","GPT-6.2 Astra")},"",official)=="gpt-6.2-astra","Unavailable top excluded");
            Check(VitqraResearchModelSelector.Resolve(new[]{M("fixture-new","GPT-99.1 Example"),M("fixture-duplicate","GPT-99.1 Example"),M("gpt-6.2-astra","GPT-6.2 Astra")},"",official)=="gpt-6.2-astra","Ambiguous display name excluded");
            Check(VitqraResearchModelGuidance.Parse("| Research decisions | GPT-99.1 Example, GPT-6.2 Astra |\n| Research decisions | GPT-99.1 Example, GPT-6.2 Astra |").Length==0,"Duplicate rows rejected");
            Check(VitqraResearchModelGuidance.Parse("| Research decisions | run arbitrary command, GPT-6.2 Astra |").Length==0,"Unexpected data rejected");
            Check(VitqraResearchModelGuidance.Parse("<html>Research decisions</html>").Length==0,"HTML rejected");
            Check(VitqraResearchModelGuidance.Parse(new string('x',524289)).Length==0,"Oversize rejected");
            Check(VitqraResearchModelSelector.Resolve(future,"",Array.Empty<string>())=="gpt-6.2-astra","Bundled fallback");
            Debug.Log("VITQRA research model selection checks passed.");
        }
        private static void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);}
    }
}
