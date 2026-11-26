using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace Vitqra
{
    // Optional user-controlled SearXNG instance. No public instance or paid fallback.
    public static class VitqraWebDiscovery
    {
        private const int MaxBytes=2097152;
        private const int MaxResults=30;
        [Serializable] private sealed class Hit { public string title,url,content; }
        [Serializable] private sealed class Response { public Hit[] results; }
        public sealed class Batch
        {
            public bool success;
            public List<VitqraChallengeDirectory.Item> items=new List<VitqraChallengeDirectory.Item>();
        }
        public static bool ValidEndpoint(string address)
        {
            if(!Uri.TryCreate(address,UriKind.Absolute,out var uri)||
               !string.IsNullOrEmpty(uri.UserInfo)||!string.IsNullOrEmpty(uri.Query)||
               !string.IsNullOrEmpty(uri.Fragment)||uri.AbsolutePath.TrimEnd('/')!="/search")return false;
            // HTTPS with normal TLS validation. HTTP only on loopback, for editor tests.
            return uri.Scheme=="https"||(uri.Scheme=="http"&&uri.IsLoopback);
        }
        public static bool ValidCandidateUrl(string address)
        {
            if(string.IsNullOrWhiteSpace(address)||address.Length>2048||
               !Uri.TryCreate(address,UriKind.Absolute,out var uri)||uri.Scheme!="https"||
               !uri.IsDefaultPort||!string.IsNullOrEmpty(uri.UserInfo)||uri.IsLoopback||
               uri.HostNameType!=UriHostNameType.Dns)return false;
            string host=uri.IdnHost.TrimEnd('.').ToLowerInvariant();
            if(!host.Contains(".")||host.EndsWith(".local")||host.EndsWith(".internal")||
               host.EndsWith(".localhost")||host.EndsWith(".test")||host.EndsWith(".invalid")||
               host.EndsWith(".example")||host.EndsWith(".onion")||host.EndsWith(".home.arpa"))return false;
            // These links are never fetched by the app; opening requires a deliberate row click.
            return uri.AbsolutePath.Trim('/').Length>0;
        }
        public static string BuildUrl(string endpoint,string term,string language,string kind)
        {
            if(!ValidEndpoint(endpoint)||string.IsNullOrWhiteSpace(term)||term.Length>120||
               (kind!="challenge"&&kind!="competition"))throw new ArgumentException("Invalid search configuration");
            return endpoint.TrimEnd('/')+"?format=json&categories=general&safesearch=2&language="+
                Uri.EscapeDataString(VitqraStrings.NormalizeLocale(language))+"&q="+
                Uri.EscapeDataString(term+" "+kind);
        }
        public static Batch Parse(string json)
        {
            var batch=new Batch();
            if(string.IsNullOrWhiteSpace(json)||json.Length>MaxBytes)return batch;
            try
            {
                var response=JsonUtility.FromJson<Response>(json);
                if(response==null||response.results==null)return batch;
                batch.success=true;
                var seen=new HashSet<string>(StringComparer.Ordinal);
                foreach(var hit in response.results)
                {
                    if(batch.items.Count>=MaxResults)break;
                    if(hit==null||!ValidCandidateUrl(hit.url)||string.IsNullOrWhiteSpace(hit.title)||
                       hit.title.Length>600)continue;
                    string title=WebUtility.HtmlDecode(Regex.Replace(hit.title,"<[^>]*>","")).Trim();
                    string excerpt=hit.content??"";
                    if(excerpt.Length>2000)excerpt=excerpt.Substring(0,2000);
                    excerpt=WebUtility.HtmlDecode(Regex.Replace(excerpt,"<[^>]*>",""));
                    if(string.IsNullOrWhiteSpace(title))continue;
                    var uri=new Uri(hit.url);
                    string path=uri.AbsolutePath.Trim('/');
                    // Reject obvious index pages. This is a heuristic, not statement verification.
                    if(Regex.IsMatch(path,@"^(challenges?|competitions?|hackathons?|search|events?|news)(/index\.html)?$",RegexOptions.IgnoreCase))continue;
                    if(!Regex.IsMatch(title+" "+excerpt+" "+path,
                       @"challenge|competition|hackathon|benchmark|contest|prize|repte|reto|desaf[ií]o|défi|wettbewerb|挑战|挑战赛|チャレンジ|تحدي|चुनौती",
                       RegexOptions.IgnoreCase|RegexOptions.CultureInvariant))continue;
                    string canonical=uri.GetLeftPart(UriPartial.Path)+uri.Query;
                    if(!seen.Add(canonical))continue;
                    batch.items.Add(new VitqraChallengeDirectory.Item {id="web:"+canonical,title=title,
                        url=canonical,keywords="",webCandidate=true});
                }
            }
            catch(ArgumentException){batch.success=false;batch.items.Clear();}
            return batch;
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
        public static IEnumerator Fetch(string endpoint,string term,string language,string kind,Action<Batch> completed)
        {
            using(var handler=new BoundedDownload())
            using(var request=new UnityWebRequest(BuildUrl(endpoint,term,language,kind),UnityWebRequest.kHttpVerbGET))
            {
                request.downloadHandler=handler;
                request.disposeDownloadHandlerOnDispose=false;
                request.timeout=15;request.redirectLimit=0;
                request.SetRequestHeader("Accept","application/json");
                yield return request.SendWebRequest();
                completed(request.result==UnityWebRequest.Result.Success?Parse(handler.ReadText()):new Batch());
            }
        }
    }
}
