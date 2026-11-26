using System;
using UnityEditor;
using UnityEngine;
namespace Vitqra
{
    public static class VitqraOllamaChecks
    {
        [MenuItem("VITQRA/Run Ollama Catalog Checks")]
        public static void Run()
        {
            Check(VitqraOllama.ValidOrigin("https://192.168.1.20:8443",out _),"Private TLS");
            foreach(string url in new[]{"http://192.168.1.20","https://ollama.com","https://127.0.0.1","https://192.168.1.20/path","https://192.168.1.20?x=1","https://192.168.1.20@8.8.8.8","https://3232235796"})
                Check(!VitqraOllama.ValidOrigin(url,out _),"Reject endpoint");
            string a=new string('a',64),b=new string('b',64);
            var models=new[]{new VitqraOllama.Model{name="fixture-a:1",digest=a,size=1},new VitqraOllama.Model{name="fixture-b:1",digest=b,size=1}};
            var reviewed=new[]{new VitqraOllama.Approved{name="fixture-b:1",digest=b},new VitqraOllama.Approved{name="fixture-a:1",digest=a}};
            Check(VitqraOllama.Select(models,reviewed,"").name=="fixture-b:1","Reviewed priority, not server order");
            Check(VitqraOllama.Select(models,reviewed,"fixture-a:1").name=="fixture-a:1","Manual");
            Check(VitqraOllama.Select(models,reviewed,"missing")==null,"Missing manual");
            models[1].digest=a;
            Check(VitqraOllama.Select(models,reviewed,"").name=="fixture-a:1","Changed digest blocked");
            models[0].remote_host="fixture";
            Check(VitqraOllama.Select(models,reviewed,"")==null,"Remote metadata blocked");
            Check(VitqraOllama.Parse("{\"models\":[{\"name\":\"fixture-a:1\",\"digest\":\""+a+"\",\"size\":1}]}").Length==1,"Catalog");
            Check(VitqraOllama.Parse("{\"models\":[]}").Length==0,"Empty");
            Check(VitqraOllama.Parse("invalid").Length==0,"Malformed");
            Check(VitqraSettings.NormalizeAi("ollama")=="ollama","Provider preference");
            Check(!new VitqraEvidenceReview.Report().CanStartAutonomousResearch,"Scientific gate unchanged");
            Debug.Log("VITQRA Ollama catalog checks passed.");
        }
        private static void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);}
    }
}
