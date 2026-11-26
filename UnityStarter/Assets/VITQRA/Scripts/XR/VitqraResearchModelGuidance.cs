using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace Vitqra
{
    // Public guidance is untrusted metadata, not instructions or executable content.
    // No account credentials, search terms, model list or research data are sent.
    public static class VitqraResearchModelGuidance
    {
        private const string Url="https://developers.openai.com/api/docs/guides/model-selection.md";
        private const int MaxBytes=512*1024;
        private static bool attempted;
        private static string[] ranked=Array.Empty<string>();
        public static bool LiveGuidanceAvailable=>ranked.Length>0;
        public static string[] Current()=> (string[])ranked.Clone();

        // Accept only a unique Markdown research-decisions table row, with a
        // bounded list of exact official display names. No inferred versions,
        // arbitrary links, free-form instructions, HTML or generated rankings.
        public static string[] Parse(string markdown)
        {
            if(string.IsNullOrEmpty(markdown)||markdown.Length>MaxBytes)return Array.Empty<string>();
            try
            {
                var matches=Regex.Matches(markdown,@"(?m)^\s*\|?\s*Research decisions\s*\|\s*([^|\r\n]+?)\s*\|?\s*$",
                    RegexOptions.CultureInvariant,TimeSpan.FromMilliseconds(200));
                if(matches.Count!=1)return Array.Empty<string>();
                var names=new List<string>();
                var seen=new HashSet<string>(StringComparer.Ordinal);
                foreach(string value in matches[0].Groups[1].Value.Split(','))
                {
                    string name=value.Trim();
                    if(name.StartsWith("**",StringComparison.Ordinal)&&name.EndsWith("**",StringComparison.Ordinal)&&name.Length>4)
                        name=name.Substring(2,name.Length-4);
                    if(!Regex.IsMatch(name,@"^GPT-[0-9]{1,3}(?:\.[0-9]{1,3})? [A-Za-z][A-Za-z0-9 -]{0,39}$",
                        RegexOptions.CultureInvariant,TimeSpan.FromMilliseconds(50))||
                        !seen.Add(name)||names.Count>=12)return Array.Empty<string>();
                    names.Add(name);
                }
                return names.Count>=2?names.ToArray():Array.Empty<string>();
            }
            catch(RegexMatchTimeoutException){return Array.Empty<string>();}
        }
        private sealed class Bounded:DownloadHandlerScript
        {
            private readonly MemoryStream bytes=new MemoryStream();
            public Bounded():base(new byte[8192]){}
            protected override bool ReceiveData(byte[] buffer,int count)
            {
                if(buffer==null||count<0||bytes.Length+count>MaxBytes)return false;
                bytes.Write(buffer,0,count);return true;
            }
            public string Read()=>new UTF8Encoding(false,true).GetString(bytes.ToArray());
        }
        public static IEnumerator RefreshAtLaunch()
        {
            if(attempted)yield break;
            attempted=true;
            using(var handler=new Bounded())
            using(var request=new UnityWebRequest(Url,UnityWebRequest.kHttpVerbGET))
            {
                request.downloadHandler=handler;request.disposeDownloadHandlerOnDispose=false;
                request.timeout=12;request.redirectLimit=0;
                request.SetRequestHeader("Accept","text/markdown");
                yield return request.SendWebRequest();
                if(request.result!=UnityWebRequest.Result.Success)yield break;
                string type=(request.GetResponseHeader("Content-Type")??"").Split(';')[0].Trim();
                if(!string.Equals(type,"text/markdown",StringComparison.OrdinalIgnoreCase)&&
                   !string.Equals(type,"text/plain",StringComparison.OrdinalIgnoreCase))yield break;
                try{ranked=Parse(handler.Read());}catch(Exception){ranked=Array.Empty<string>();}
            }
        }
    }
}
