using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace Vitqra
{
    // Public source snapshots only. No executable content, credential handling or
    // research claims. The application never submits or redistributes evidence.
    public static class VitqraEvidence
    {
        public const int MaxHtml=2*1024*1024, MaxPdf=16*1024*1024, MaxJobs=12;
        public const long MaxStore=128L*1024*1024;
        private static readonly TimeSpan RegexLimit=TimeSpan.FromSeconds(1);
        private static readonly HashSet<string> Names=new HashSet<string>(StringComparer.Ordinal)
            {"source.html.txt","source-preview.txt","public-sample.txt","official-notice.pdf"};
        [Serializable] public sealed class Asset
        {
            public string name,sourceUrl,kind,sha256,rights;
            public long bytes;
        }
        [Serializable] public sealed class Manifest
        {
            public int version=1;
            public string challengeId,title,sourceUrl,retrievedUtc;
            public bool educational,previewTruncated;
            public string[] repositoryCandidates;
            public Asset[] assets;
            public string scientificStatus="not_verified";
        }
        public sealed class Result { public bool success,partial,cached; }

        public static bool Eligible(VitqraChallengeDirectory.Item item)
        {
            return item!=null&&!item.webCandidate&&!string.IsNullOrEmpty(item.id)&&
                VitqraChallengeDirectory.ValidLink(item)&&VitqraWebReader.CanRead(item.url);
        }
        public static bool ValidPdf(VitqraChallengeDirectory.Item item)
        {
            if(!Eligible(item)||item.educational||
                !Regex.IsMatch(item.id,@"^fr-(?:C[0-9]+-)?[0-9]{4}-[0-9]{4,6}$")||
                !DateTime.TryParseExact(item.publicationDate,"yyyy-MM-dd",CultureInfo.InvariantCulture,
                    DateTimeStyles.None,out _))return false;
            string expected="https://www.govinfo.gov/content/pkg/FR-"+item.publicationDate+"/pdf/"+item.id.Substring(3)+".pdf";
            return string.Equals(item.officialPdf,expected,StringComparison.Ordinal);
        }
        public static bool PdfSignature(byte[] data)
        {
            return data!=null&&data.Length>=5&&data[0]==37&&data[1]==80&&data[2]==68&&data[3]==70&&data[4]==45;
        }
        public static string SampleDataset(string html)
        {
            if(string.IsNullOrEmpty(html)||html.Length>MaxHtml)return null;
            try
            {
                var m=Regex.Match(html,@"<h([1-6])\b[^>]*>\s*Sample Dataset\s*</h\1>\s*<pre\b[^>]*>(.*?)</pre>",
                    RegexOptions.IgnoreCase|RegexOptions.Singleline|RegexOptions.CultureInvariant,RegexLimit);
                if(!m.Success)return null;
                string sample=WebUtility.HtmlDecode(m.Groups[2].Value).Trim();
                return sample.Length>0&&sample.Length<=64000&&sample.IndexOf('<')<0&&sample.IndexOf('\0')<0?sample:null;
            }
            catch(RegexMatchTimeoutException){return null;}
        }
        public static string[] RepositoryLinks(string html)
        {
            var links=new HashSet<string>(StringComparer.Ordinal);
            if(string.IsNullOrEmpty(html)||html.Length>MaxHtml)return new string[0];
            try
            {
                foreach(Match m in Regex.Matches(html,@"https://(?:zenodo\.org/(?:record|records)/[0-9]+|github\.com/[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+)(?=[\s""'<>/#?]|$)",RegexOptions.CultureInvariant,RegexLimit))
                {links.Add(m.Value);if(links.Count>=30)break;}
            }
            catch(RegexMatchTimeoutException){}
            return links.OrderBy(x=>x,StringComparer.Ordinal).ToArray();
        }
        private static string Hex(byte[] data)=>BitConverter.ToString(data).Replace("-","").ToLowerInvariant();
        private static string Hash(byte[] data)
        {using(var sha=SHA256.Create())return Hex(sha.ComputeHash(data));}
        private static string FileHash(string path)
        {using(var sha=SHA256.Create())using(var file=File.OpenRead(path))return Hex(sha.ComputeHash(file));}
        public static string Folder(string root,string id,string url)=>Path.Combine(root,Hash(Encoding.UTF8.GetBytes(id+"\n"+url)));
        public static Manifest ReadVerified(string folder,VitqraChallengeDirectory.Item item)
        {
            try
            {
                string path=Path.Combine(folder,"manifest.json");
                if(!File.Exists(path)||new FileInfo(path).Length>131072)return null;
                var m=JsonUtility.FromJson<Manifest>(File.ReadAllText(path));
                if(m==null||m.version!=1||m.challengeId!=item.id||m.sourceUrl!=item.url||
                    m.educational!=item.educational||m.scientificStatus!="not_verified"||
                    m.assets==null||m.assets.Length<2||m.assets.Length>4)return null;
                var names=new HashSet<string>(StringComparer.Ordinal);
                foreach(var a in m.assets)
                {
                    if(a==null||!Names.Contains(a.name)||!names.Add(a.name)||a.bytes<=0||a.bytes>MaxPdf)return null;
                    if(a.name=="official-notice.pdf")
                    {if(!ValidPdf(item)||a.sourceUrl!=item.officialPdf)return null;}
                    else if(a.sourceUrl!=item.url||(a.name=="public-sample.txt"&&!item.educational))return null;
                    string file=Path.Combine(folder,a.name);
                    if(!File.Exists(file)||new FileInfo(file).Length!=a.bytes||FileHash(file)!=a.sha256)return null;
                }
                return names.Contains("source.html.txt")&&names.Contains("source-preview.txt")?m:null;
            }
            catch(Exception){return null;}
        }
        private static void WriteAsset(string folder,List<Asset> assets,string name,byte[] bytes,string source,string kind)
        {
            if(!Names.Contains(name)||bytes==null||bytes.Length==0)throw new IOException();
            File.WriteAllBytes(Path.Combine(folder,name),bytes);
            assets.Add(new Asset {name=name,bytes=bytes.LongLength,sourceUrl=source,kind=kind,
                rights="source_terms_apply; redistribution_not_approved",sha256=Hash(bytes)});
        }
        private sealed class BoundedDownload:DownloadHandlerScript
        {
            private readonly MemoryStream buffer=new MemoryStream();
            private readonly int limit;
            public BoundedDownload(int max):base(new byte[8192]){limit=max;}
            protected override bool ReceiveData(byte[] bytes,int count)
            {
                if(bytes==null||count<0||buffer.Length+count>limit)return false;
                buffer.Write(bytes,0,count);return true;
            }
            public byte[] Read()=>buffer.ToArray();
        }
        private static IEnumerator Download(string url,int limit,string mime,Action<UnityWebRequest> active,Action<byte[]> done)
        {
            using(var handler=new BoundedDownload(limit))
            using(var request=new UnityWebRequest(url,UnityWebRequest.kHttpVerbGET))
            {
                request.downloadHandler=handler;request.disposeDownloadHandlerOnDispose=false;
                request.timeout=25;request.redirectLimit=0;request.SetRequestHeader("Accept",mime);
                active(request);
                try
                {
                    yield return request.SendWebRequest();
                    string type=(request.GetResponseHeader("Content-Type")??"").Split(';')[0].Trim();
                    bool valid=string.Equals(type,mime,StringComparison.OrdinalIgnoreCase)||
                        (mime=="text/html"&&string.Equals(type,"application/xhtml+xml",StringComparison.OrdinalIgnoreCase));
                    done(request.result==UnityWebRequest.Result.Success&&valid?handler.Read():null);
                }
                finally{active(null);}
            }
        }
        public static IEnumerator Acquire(VitqraChallengeDirectory.Item item,string root,Action<UnityWebRequest> active,Action<Result> done)
        {
            var result=new Result();
            if(!Eligible(item)){done(result);yield break;}
            string folder=Folder(root,item.id,item.url);
            bool canWrite=false;
            try
            {
                Directory.CreateDirectory(root);
                if(Directory.Exists(folder))
                {
                    var old=ReadVerified(folder,item);
                    if(old!=null)
                    {
                        result.success=true;result.cached=true;
                        result.partial=!old.assets.Any(a=>a.name==(item.educational?"public-sample.txt":"official-notice.pdf"));
                    }
                }
                else
                {
                    long used=0;
                    foreach(string file in Directory.EnumerateFiles(root,"*",SearchOption.AllDirectories))
                    {used+=new FileInfo(file).Length;if(used>MaxStore)break;}
                    canWrite=used+MaxHtml*3L+MaxPdf+131072<=MaxStore;
                }
            }
            catch(Exception){}
            if(!canWrite){done(result);yield break;}
            byte[] raw=null,pdf=null;
            yield return Download(item.url,MaxHtml,"text/html",active,x=>raw=x);
            if(raw==null||raw.Length==0){done(result);yield break;}
            string html=Encoding.UTF8.GetString(raw);
            var preview=VitqraWebReader.Extract(html);
            if(!preview.success){done(result);yield break;}
            string sample=item.educational?SampleDataset(html):null;
            if(ValidPdf(item))yield return Download(item.officialPdf,MaxPdf,"application/pdf",active,x=>pdf=x);
            string stage=folder+".pending-"+Guid.NewGuid().ToString("N");
            try
            {
                Directory.CreateDirectory(stage);
                var assets=new List<Asset>();
                WriteAsset(stage,assets,"source.html.txt",raw,item.url,"original_html_bytes");
                WriteAsset(stage,assets,"source-preview.txt",Encoding.UTF8.GetBytes(preview.text),item.url,"derived_text_preview");
                if(!string.IsNullOrEmpty(sample))
                    WriteAsset(stage,assets,"public-sample.txt",Encoding.UTF8.GetBytes(sample),item.url,"educational_sample");
                if(PdfSignature(pdf))
                    WriteAsset(stage,assets,"official-notice.pdf",pdf,item.officialPdf,"official_notice");
                var manifest=new Manifest {challengeId=item.id,title=item.title,sourceUrl=item.url,
                    retrievedUtc=DateTime.UtcNow.ToString("o"),educational=item.educational,previewTruncated=preview.truncated,
                    repositoryCandidates=RepositoryLinks(html),assets=assets.ToArray()};
                File.WriteAllText(Path.Combine(stage,"manifest.json"),JsonUtility.ToJson(manifest,true),new UTF8Encoding(false));
                Directory.Move(stage,folder);
                result.success=true;result.partial=item.educational?string.IsNullOrEmpty(sample):!PdfSignature(pdf);
            }
            catch(Exception)
            {try{if(Directory.Exists(stage))Directory.Delete(stage,true);}catch(Exception){}}
            done(result);
        }
    }
}
