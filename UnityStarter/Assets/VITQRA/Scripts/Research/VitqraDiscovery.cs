using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace Vitqra
{
    // Public announcement discovery only. Publication date is not a deadline.
    public static class VitqraDiscovery
    {
        public const string Endpoint = "https://www.federalregister.gov/api/v1/documents.json";
        public const int MaxBytes = 2097152;
        public const int PageSize = 40;
        public static readonly string[] Topics = {"prize competition","challenge","hackathon"};
        public static readonly string[] PortalUrls = {
            "https://www.nih.gov/challenges",
            "https://challenges.synapse.org/OpenChallenges",
            "https://www.spaceappschallenge.org/"
        };
        [Serializable] public sealed class Notice
        {
            public string document_number,title,abstract_text,html_url,publication_date,type;
        }
        [Serializable] private sealed class RawNotice
        {
            public string document_number,title,html_url,publication_date,type;
            // The API's abstract field is not used; preserve the original title only.
        }
        [Serializable] private sealed class Response { public RawNotice[] results; public int count; }
        public sealed class Batch { public List<Notice> items=new List<Notice>(); public bool success; public bool limited; }
        public static string BuildUrl(string fixedTerm)
        {
            if(Array.IndexOf(Topics,fixedTerm)<0)throw new ArgumentException("Unapproved search term");
            return Endpoint+"?per_page="+PageSize+"&order=newest&conditions%5Bterm%5D="+
                Uri.EscapeDataString(fixedTerm)+"&conditions%5Btype%5D%5B%5D=NOTICE"+
                "&fields%5B%5D=document_number&fields%5B%5D=title&fields%5B%5D=html_url"+
                "&fields%5B%5D=publication_date&fields%5B%5D=type";
        }
        public static bool ValidSource(string url)
        {
            return Uri.TryCreate(url,UriKind.Absolute,out var uri)&&uri.Scheme=="https"&&
                uri.Host=="www.federalregister.gov"&&uri.IsDefaultPort&&string.IsNullOrEmpty(uri.UserInfo)&&
                uri.AbsolutePath.StartsWith("/documents/",StringComparison.Ordinal);
        }
        public static Batch Parse(string json)
        {
            var batch=new Batch();
            if(string.IsNullOrEmpty(json)||json.Length>MaxBytes)return batch;
            try
            {
                var response=JsonUtility.FromJson<Response>(json);
                if(response==null||response.results==null)return batch;
                batch.success=true;
                batch.limited=response.count>response.results.Length;
                foreach(var x in response.results)
                {
                    if(x==null||string.IsNullOrWhiteSpace(x.title)||x.title.Length>1200||
                       string.IsNullOrWhiteSpace(x.document_number)||x.document_number.Length>80||
                       !ValidSource(x.html_url)||!string.Equals(x.type,"Notice",StringComparison.OrdinalIgnoreCase)||
                       !DateTime.TryParseExact(x.publication_date,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out _))continue;
                    if(!Regex.IsMatch(x.title,@"\b(challenge|challenges|hackathon|hackathons|prize\s+competition|prize\s+competitions)\b",RegexOptions.IgnoreCase|RegexOptions.CultureInvariant))continue;
                    batch.items.Add(new Notice {document_number=x.document_number,title=x.title,
                        html_url=x.html_url,publication_date=x.publication_date,type=x.type});
                }
            }
            catch(ArgumentException){batch.success=false;batch.items.Clear();}
            return batch;
        }
        public static List<Notice> Filter(IEnumerable<Notice> notices,string query)
        {
            var list=new List<Notice>();
            var seen=new HashSet<string>(StringComparer.Ordinal);
            var words=(query??"").Trim().Split(new[]{' ','\t','\n'},StringSplitOptions.RemoveEmptyEntries);
            foreach(var n in notices)
            {
                if(n==null||string.IsNullOrWhiteSpace(n.title)||!ValidSource(n.html_url))continue;
                bool matches=true;
                foreach(var word in words)
                    if(CultureInfo.InvariantCulture.CompareInfo.IndexOf(n.title,word,CompareOptions.IgnoreCase|CompareOptions.IgnoreNonSpace)<0)
                    {matches=false;break;}
                if(matches&&seen.Add(n.document_number))list.Add(n);
            }
            list.Sort((a,b)=>string.CompareOrdinal(b.publication_date,a.publication_date));
            return list;
        }
        // Reject oversized responses while streaming, before accumulating unbounded data.
        private sealed class BoundedDownload : DownloadHandlerScript
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
        public static IEnumerator Fetch(string term,Action<Batch> completed)
        {
            using(var handler=new BoundedDownload())
            using(var request=new UnityWebRequest(BuildUrl(term),UnityWebRequest.kHttpVerbGET))
            {
                request.downloadHandler=handler;
                request.disposeDownloadHandlerOnDispose=false;
                request.timeout=15;
                request.redirectLimit=0;
                request.SetRequestHeader("Accept","application/json");
                yield return request.SendWebRequest();
                completed(request.result==UnityWebRequest.Result.Success?Parse(handler.ReadText()):new Batch());
            }
        }
    }
}
