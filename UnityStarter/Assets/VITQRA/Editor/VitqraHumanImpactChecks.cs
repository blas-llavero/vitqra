using System;
using UnityEditor;
using UnityEngine;
namespace Vitqra
{
    public static class VitqraHumanImpactChecks
    {
        [MenuItem("VITQRA/Run Human Impact Checks")]
        public static void Run()
        {
            var fields=VitqraHumanImpact.Parse("CANDIDATE=KMP\nBENEFIT=Research question?\nHARM=UNKNOWN\nCOST=Estimate needed");
            Check(fields.Length==8&&fields[0]=="Research question?"&&fields[1]==null&&fields[4]=="Estimate needed","Known fields");
            Check(VitqraHumanImpact.Parse("BENEFIT=x\nBENEFIT=y")[0]==null,"Duplicate");
            Check(VitqraHumanImpact.Parse("BENEFIT=<tag>")[0]==null,"Markup");
            Check(VitqraHumanImpact.Parse("BENEFIT=\u202Etest")[0]==null,"Format control");
            Check(VitqraHumanImpact.Parse("BENEFIT="+new string('a',241))[0]==null,"Oversize field");
            Check(VitqraHumanImpact.Parse(new string('a',6001))[0]==null,"Oversize response");
            Check(VitqraHumanImpact.Parse("SAFETY=approved")[0]==null,"No release decision field");
            var draft=new VitqraResearchDraft.Draft{critique="BENEFIT=Fixture question",draftSha256="fixture"};
            Check(!VitqraHumanImpact.Describe(draft,"en").Contains("Fixture question"),"Legacy not relabeled");
            draft.impactProtocol=VitqraHumanImpact.Protocol;
            Check(VitqraHumanImpact.Describe(draft,"en").Contains("Fixture question"),"New protocol");
            Check(VitqraHumanImpact.Describe(null,"en").Contains("Not assessed / unknown"),"Missing evidence");
            foreach(string locale in new[]{"en","ca","es","zh","hi","fr","de","pt","ar","ja"})
                Check(!string.IsNullOrEmpty(VitqraHumanImpact.Heading(locale))&&!string.IsNullOrEmpty(VitqraHumanImpact.Describe(null,locale)),"Localization");
            Check(!new VitqraEvidenceReview.Report().CanStartAutonomousResearch,"Scientific gate closed");
            Debug.Log("VITQRA human impact checks passed.");
        }
        private static void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);}
    }
}
