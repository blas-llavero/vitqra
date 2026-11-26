using System;
using UnityEditor;
using UnityEngine;

namespace Vitqra
{
    public static class VitqraWebReaderChecks
    {
        [MenuItem("VITQRA/Run Web Reader Checks")]
        public static void Run()
        {
            Check(VitqraWebReader.CanRead("https://rosalind.info/problems/dna/"),"Direct educational link");
            Check(VitqraWebReader.CanRead("https://www.federalregister.gov/documents/fixture"),"Notice link");
            Check(!VitqraWebReader.CanRead("https://rosalind.info.example.org/problems/dna/"),"Foreign domain");
            Check(!VitqraWebReader.CanRead("http://rosalind.info/problems/dna/"),"HTTPS only");
            Check(!VitqraWebReader.CanRead("https://rosalind.info/problems/dna/?x=1"),"No query strings");
            var text=VitqraWebReader.Extract("<html><head>hidden</head><body><p>DNA &amp; RNA</p><script>doNotRun()</script><p>Text</p></body></html>");
            Check(text.success&&text.text.Contains("DNA & RNA")&&text.text.Contains("Text"),"Text extraction");
            Check(!text.text.Contains("doNotRun")&&!text.text.Contains("hidden"),"Remove script and head");
            var pages=VitqraWebReader.Pages(new string('a',1000));
            Check(pages.Count==3&&string.Concat(pages).Length==1000,"Bounded pagination");
            Check(!VitqraWebReader.Extract("").success,"Empty response");
            Debug.Log("VITQRA web reader checks passed.");
        }
        private static void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);}
    }
}
