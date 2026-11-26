using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.Networking;

namespace Vitqra
{
    // Read-only, bounded text preview. No DOM, JavaScript, subresources or cookies
    // are required by this implementation. Text is never interpreted as instructions.
    public static class VitqraWebReader
    {
        public const int MaxBytes=2097152, MaxText=48000, PageElements=480;
        private static readonly TimeSpan RegexLimit=TimeSpan.FromSeconds(1);
        public sealed class Result
        {
            public bool success,truncated;
            public string text="";
        }
        public static bool CanRead(string address)
        {
            if(!Uri.TryCreate(address,UriKind.Absolute,out var uri)||uri.Scheme!="https"||
                !uri.IsDefaultPort||!string.IsNullOrEmpty(uri.UserInfo)||!string.IsNullOrEmpty(uri.Query))return false;
            if(uri.Host=="rosalind.info")
                return Regex.IsMatch(uri.AbsolutePath,@"^/problems/[a-z0-9-]+/$",RegexOptions.CultureInvariant);
            return VitqraDiscovery.ValidSource(address);
        }

        public static Result Extract(string html)
        {
            var result=new Result();
            if(string.IsNullOrWhiteSpace(html)||html.Length>MaxBytes)return result;
            try
            {
                // This is a display heuristic, not a full HTML parser or statement extractor.
                var options=RegexOptions.IgnoreCase|RegexOptions.Singleline|RegexOptions.CultureInvariant;
                string clean=Regex.Replace(html,@"<!--.*?(?:-->|\z)"," ",options,RegexLimit);
                clean=Regex.Replace(clean,@"<(script|style|head|noscript|template|iframe|svg)\b[^>]*>.*?(?:</\1\s*>|\z)"," ",options,RegexLimit);
                clean=Regex.Replace(clean,@"</?(p|div|h[1-6]|li|br|tr|section|article|pre|table|ul|ol)\b[^>]*>","\n",options,RegexLimit);
                clean=Regex.Replace(clean,@"<[^>]*>"," ",options,RegexLimit);
                clean=WebUtility.HtmlDecode(clean);
                clean=Regex.Replace(clean,@"[^\S\r\n]+"," ",RegexOptions.CultureInvariant,RegexLimit);
                clean=Regex.Replace(clean,@"\s*\n\s*","\n",RegexOptions.CultureInvariant,RegexLimit).Trim();
                if(clean.Length>MaxText)
                {
                    int cut=MaxText;
                    if(char.IsHighSurrogate(clean[cut-1]))cut--;
                    clean=clean.Substring(0,cut);result.truncated=true;
                }
                result.text=clean;result.success=clean.Length>0;
            }
            catch(RegexMatchTimeoutException){ }
            return result;
        }

        public static List<string> Pages(string text)
        {
            var pages=new List<string>();
            if(string.IsNullOrEmpty(text))return pages;
            int[] offsets=StringInfo.ParseCombiningCharacters(text);
            int start=0;
            while(start<offsets.Length)
            {
                int end=Math.Min(start+PageElements,offsets.Length);
                // Prefer a whitespace boundary without creating very short pages.
                if(end<offsets.Length)
                    for(int i=end-1;i>start+PageElements/2;i--)
                        if(char.IsWhiteSpace(text[offsets[i]])){end=i+1;break;}
                int endOffset=end==offsets.Length?text.Length:offsets[end];
                pages.Add(text.Substring(offsets[start],endOffset-offsets[start]).Trim());
                start=end;
            }
            return pages;
        }

        private sealed class BoundedDownload:DownloadHandlerScript
        {
            private readonly System.IO.MemoryStream data=new System.IO.MemoryStream();
            public BoundedDownload():base(new byte[8192]){}
            protected override bool ReceiveData(byte[] bytes,int length)
            {
                if(bytes==null||length<0||data.Length+length>MaxBytes)return false;
                data.Write(bytes,0,length);return true;
            }
            public string ReadText()=>Encoding.UTF8.GetString(data.ToArray());
        }

        public static IEnumerator Fetch(string address,Action<UnityWebRequest> requestChanged,Action<Result> completed)
        {
            if(!CanRead(address)){completed(new Result());yield break;}
            using(var handler=new BoundedDownload())
            using(var request=new UnityWebRequest(address,UnityWebRequest.kHttpVerbGET))
            {
                request.downloadHandler=handler;request.disposeDownloadHandlerOnDispose=false;
                request.timeout=15;request.redirectLimit=0;
                request.SetRequestHeader("Accept","text/html, application/xhtml+xml");
                requestChanged(request);
                try
                {
                    yield return request.SendWebRequest();
                    string type=request.GetResponseHeader("Content-Type")??"";
                    bool html=type.StartsWith("text/html",StringComparison.OrdinalIgnoreCase)||
                        type.StartsWith("application/xhtml+xml",StringComparison.OrdinalIgnoreCase);
                    completed(request.result==UnityWebRequest.Result.Success&&html?
                        Extract(handler.ReadText()):new Result());
                }
                finally{requestChanged(null);}
            }
        }
    }
}
