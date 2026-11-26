using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using D=Vitqra.VitqraDiscoveryStrings.Key;

namespace Vitqra
{
    // No external PDFs, archive extraction, external storage or automatic sharing.
    public static class VitqraResearchPackage
    {
        private const int MaxText=262144, MaxPdf=8*1024*1024, MaxZip=10*1024*1024;
        private const long MaxStore=32L*1024*1024;
        private static readonly UTF8Encoding Utf8=new UTF8Encoding(false,true);
        private static readonly SemaphoreSlim Gate=new SemaphoreSlim(1,1);
        private static string T(string locale,D key)=>VitqraDiscoveryStrings.Get(locale,key);
        public sealed class Content
        {
            public string title,body,warning,html,identity,reproducibility,reviewRequest;
        }
        public sealed class Page
        {
            public int count;
            public byte[] png;
        }
        public static bool Available
        {
            get{
#if UNITY_ANDROID && !UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }
        private static string Hash(byte[] bytes)
        {using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").ToLowerInvariant();}
        private static byte[] Read(string path,int max)
        {
            using(var f=File.OpenRead(path))
            {
                if(f.Length<=0||f.Length>max)throw new IOException("File limit");
                var b=new byte[(int)f.Length];int n=0;
                while(n<b.Length){int read=f.Read(b,n,b.Length-n);if(read<=0)throw new IOException();n+=read;}
                if(f.ReadByte()!=-1)throw new IOException();
                return b;
            }
        }
        public static Content Build(string evidence,string results,string reports,string drafts,VitqraChallengeDirectory.Item item,string language)
        {
            try
            {
                if(VitqraResearchReport.Load(evidence,results,reports,item,language)==null)return null;
                var report=VitqraResearchReport.Build(evidence,results,item,language);
                if(report==null)return null;
                string benchmark=VitqraMotifBenchmark.Load(evidence,results,item);
                if(benchmark!=null)report.sections.Add(new VitqraResearchReport.Section{heading=T(language,D.BenchmarkHeading),body=T(language,D.BenchmarkScope)+"\n"+benchmark});
                var draft=VitqraResearchDraft.Load(evidence,drafts,item,language);
                string optimization=benchmark==null?null:VitqraMotifOptimization.Describe(draft);
                if(optimization!=null)report.sections.Add(new VitqraResearchReport.Section{heading=T(language,D.OptimizationHeading),body=T(language,D.OptimizationScope)+"\n"+optimization});
                report.sections.Add(new VitqraResearchReport.Section{heading=VitqraHumanImpact.Heading(language),body=VitqraHumanImpact.Describe(draft,language)});
                string warning=T(language,D.DraftWarning);
                if(draft!=null)
                {
                    report.sections.Add(new VitqraResearchReport.Section{heading=warning,body=
                        T(language,D.DraftAlternatives)+"\n"+draft.alternatives+"\n\n"+
                        T(language,D.DraftImpact)+"\n"+draft.critique+"\n\n"+warning+
                        "\n"+draft.ModelIdentity+"\nSHA-256 (source): "+draft.sourceSha256});
                }
                string audit=VitqraReproducibility.Build(evidence,results,drafts,item,language);
                if(audit==null)return null;
                report.sections.Add(new VitqraResearchReport.Section{heading=VitqraReproducibility.Heading(language),body=VitqraReproducibility.Scope(language)+"\n"+audit});
                string body=VitqraResearchReport.PlainText(report),html=VitqraResearchReport.Html(report);
                if(body.Length>40000||Utf8.GetByteCount(html)>MaxText||Utf8.GetByteCount(body)>MaxText)return null;
                // Stable path for the exact language, source, evidence and draft contents.
                string reviewRequest=VitqraReviewDossier.Build(body,html,audit);
                if(reviewRequest==null)return null;
                string identity=Hash(Utf8.GetBytes("v0.5.6a\n"+body+"\n"+html+"\n"+warning+"\n"+reviewRequest));
                return new Content{title=report.title,body=body,html=html,warning=warning,identity=identity,reproducibility=audit,reviewRequest=reviewRequest};
            }
            catch(Exception){return null;}
        }
        private static string Folder(string root,Content c)=>Path.Combine(root,c.identity);
        private static byte[] Manifest(Dictionary<string,byte[]> files)
        {
            var b=new StringBuilder("VITQRA research archive v0.5.3\nScope: local research only; no scientific or clinical approval.\nOriginal evidence retained separately; source terms apply.\n");
            foreach(var f in files.OrderBy(x=>x.Key,StringComparer.Ordinal))
                b.Append(f.Key).Append(" | ").Append(f.Value.Length.ToString(CultureInfo.InvariantCulture)).Append(" | ").Append(Hash(f.Value)).Append('\n');
            return Utf8.GetBytes(b.ToString());
        }
        private static Dictionary<string,byte[]> Files(Content c,byte[] pdf)
        {
            if(pdf.Length>MaxPdf||!VitqraEvidence.PdfSignature(pdf))throw new IOException("Invalid PDF");
            var files=new Dictionary<string,byte[]>(StringComparer.Ordinal){
                {"report.txt",Utf8.GetBytes(c.body)},{"report.html",Utf8.GetBytes(c.html)},{"report.pdf",pdf}};
            if(string.IsNullOrEmpty(c.reproducibility)||Utf8.GetByteCount(c.reproducibility)>32768)throw new IOException("Invalid reproducibility record");
            files.Add("reproducibility.txt",Utf8.GetBytes(c.reproducibility));
            if(!VitqraReviewDossier.Matches(c.reviewRequest,c.body,c.html,c.reproducibility))throw new IOException("Invalid unsigned review request");
            files.Add("review-request.txt",Utf8.GetBytes(c.reviewRequest));
            files.Add("manifest.txt",Manifest(files));
            return files;
        }
        private static bool ZipMatches(byte[] zip,Dictionary<string,byte[]> files)
        {
            using(var stream=new MemoryStream(zip,false))
            using(var archive=new ZipArchive(stream,ZipArchiveMode.Read,false))
            {
                if(archive.Entries.Count!=files.Count)return false;
                var seen=new HashSet<string>(StringComparer.Ordinal);
                foreach(var entry in archive.Entries)
                {
                    if(!seen.Add(entry.FullName)||!files.TryGetValue(entry.FullName,out var expected)||entry.Length!=expected.Length)return false;
                    using(var input=entry.Open())
                    {
                        for(int i=0;i<expected.Length;i++)if(input.ReadByte()!=expected[i])return false;
                        if(input.ReadByte()!=-1)return false;
                    }
                }
                return true;
            }
        }
        private static byte[] VerifiedPdf(string folder,Content c)
        {
            var pdf=Read(Path.Combine(folder,"report.pdf"),MaxPdf);
            var files=Files(c,pdf);
            foreach(var file in files)
                if(!Read(Path.Combine(folder,file.Key),file.Key=="report.pdf"?MaxPdf:MaxText).SequenceEqual(file.Value))return null;
            return ZipMatches(Read(Path.Combine(folder,"research.zip"),MaxZip),files)?pdf:null;
        }
#if UNITY_ANDROID && !UNITY_EDITOR
        private static byte[] NativeBytes(AndroidJavaClass api,string method,params object[] args)
        {
            var signed=api.CallStatic<sbyte[]>(method,args);
            return signed==null?null:Array.ConvertAll(signed,x=>unchecked((byte)x));
        }
        private static Page RenderBytes(AndroidJavaClass api,string root,byte[] pdf,int index)
        {
            Directory.CreateDirectory(root);
            string temp=Path.Combine(root,".render-"+Guid.NewGuid().ToString("N")+".pdf");
            try
            {
                File.WriteAllBytes(temp,pdf);
                int n=api.CallStatic<int>("pageCount",temp);
                if(n<1||n>48||index<0||index>=n)return null;
                byte[] image=NativeBytes(api,"render",temp,index);
                if(image==null||image.Length<8||image.Length>4*1024*1024||image[0]!=137||image[1]!=80||image[2]!=78||image[3]!=71)return null;
                return new Page{count=n,png=image};
            }
            finally{if(File.Exists(temp))File.Delete(temp);}
        }
#endif
        public static async Task<bool> Save(string root,Content c,CancellationToken token)
        {
            if(!Available||c==null)return false;
            bool locked=false;
            try
            {
                await Gate.WaitAsync(token);locked=true;
                return await Task.Run(()=>{
#if UNITY_ANDROID && !UNITY_EDITOR
                    token.ThrowIfCancellationRequested();
                    if(AndroidJNI.AttachCurrentThread()!=0)return false;
                    string stage=null;
                    try
                    {
                        using(var api=new AndroidJavaClass("com.blasllavero.vitqra.pdf.VitqraPdf"))
                        {
                            string target=Folder(root,c);
                            if(Directory.Exists(target))
                            {
                                var old=VerifiedPdf(target,c);
                                return old!=null&&RenderBytes(api,root,old,0)!=null;
                            }
                            Directory.CreateDirectory(root);
                            long used=0;
                            foreach(string f in Directory.EnumerateFiles(root,"*",SearchOption.AllDirectories))
                            {used+=new FileInfo(f).Length;if(used>MaxStore)return false;}
                            byte[] pdf=NativeBytes(api,"generate",c.title,c.body,c.warning);
                            if(pdf==null||pdf.Length>MaxPdf||!VitqraEvidence.PdfSignature(pdf))return false;
                            if(RenderBytes(api,root,pdf,0)==null)return false;
                            token.ThrowIfCancellationRequested();
                            var files=Files(c,pdf);
                            stage=target+".pending-"+Guid.NewGuid().ToString("N");
                            Directory.CreateDirectory(stage);
                            foreach(var f in files)File.WriteAllBytes(Path.Combine(stage,f.Key),f.Value);
                            string zip=Path.Combine(stage,"research.zip");
                            using(var stream=File.Create(zip))
                            using(var archive=new ZipArchive(stream,ZipArchiveMode.Create))
                                foreach(var f in files)
                                {
                                    var entry=archive.CreateEntry(f.Key,CompressionLevel.Optimal);
                                    using(var output=entry.Open())output.Write(f.Value,0,f.Value.Length);
                                }
                            long size=files.Sum(f=>(long)f.Value.Length)+new FileInfo(zip).Length;
                            if(used+size>MaxStore||VerifiedPdf(stage,c)==null)return false;
                            token.ThrowIfCancellationRequested();
                            Directory.Move(stage,target);stage=null;return true;
                        }
                    }
                    finally
                    {
                        if(stage!=null)try{Directory.Delete(stage,true);}catch(Exception){}
                        AndroidJNI.DetachCurrentThread();
                    }
#else
                    return false;
#endif
                },token);
            }
            catch(Exception){return false;}
            finally{if(locked)Gate.Release();}
        }
        public static async Task<Page> Render(string root,Content c,int index,CancellationToken token)
        {
            if(!Available||c==null)return null;
            bool locked=false;
            try
            {
                await Gate.WaitAsync(token);locked=true;
                return await Task.Run(()=>{
#if UNITY_ANDROID && !UNITY_EDITOR
                    token.ThrowIfCancellationRequested();
                    if(AndroidJNI.AttachCurrentThread()!=0)return null;
                    try
                    {
                        byte[] pdf=VerifiedPdf(Folder(root,c),c);
                        if(pdf==null)return null;
                        using(var api=new AndroidJavaClass("com.blasllavero.vitqra.pdf.VitqraPdf"))
                            return RenderBytes(api,root,pdf,index);
                    }
                    finally{AndroidJNI.DetachCurrentThread();}
#else
                    return (Page)null;
#endif
                },token);
            }
            catch(Exception){return null;}
            finally{if(locked)Gate.Release();}
        }
    }
}
