using System;
using UnityEditor;
using UnityEngine;

namespace Vitqra
{
    public static class VitqraBrowserChecks
    {
        [MenuItem("VITQRA/Run Graphical Browser Checks")]
        public static void Run()
        {
            Check(VitqraBrowserAddress.Valid("https://rosalind.info/"),"HTTPS");
            Check(!VitqraBrowserAddress.Valid("http://rosalind.info/"),"HTTP denied");
            Check(!VitqraBrowserAddress.Valid("https://192.168.1.1/"),"IP denied");
            Check(!VitqraBrowserAddress.Valid("https://service.local/"),"Local suffix denied");
            Check(!VitqraBrowserAddress.Valid("https://user@rosalind.info/"),"Credentials denied");
            Check(!VitqraBrowserAddress.Valid("file:///data/test"),"File denied");
            Check(VitqraDiscoveryPanel.GraphicUv(new Vector2(0,-.065f),out var uv)&&
                Vector2.Distance(uv,new Vector2(.5f,.5f))<.001f,"UV center");
            Check(!VitqraDiscoveryPanel.GraphicUv(new Vector2(0,.4f),out _),"Toolbar outside webpage");
            Debug.Log("VITQRA graphical browser checks passed.");
        }
        private static void Check(bool value,string name){if(!value)throw new InvalidOperationException(name);}
    }
}
