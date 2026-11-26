using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Vitqra
{
    // User-operated LAN server, HTTPS only. No cloud, downloads, tools or arbitrary prompts.
    // The administrator must separately disable cloud features and restrict server egress.
    public static class VitqraOllama
    {
        [Serializable] public sealed class Approved
        {
            public string name,digest;
        }
        [Serializable] public sealed class Model
        {
            public string name,digest,remote_host,remote_model;
            public long size;
        }
        [Serializable] private sealed class CatalogResponse {public Model[] models;}
        [Serializable] private sealed class Message {public string role,content;}
        [Serializable] private sealed class Options {public int num_predict=1024;public int num_ctx=4096;}
        [Serializable] private sealed class Request
        {
            public string model;
            public bool stream=false;
            public int keep_alive=0;
            public Options options=new Options();
            public Message[] messages;
        }
        [Serializable] private sealed class Response {public string model,done_reason,error;public bool done;public Message message;}
        public sealed class DraftReply {public string model,digest,alternatives,critique;}
        public enum State {Unconfigured,Loading,Ready,Unavailable}
        private static string origin="";
        private static Approved[] approved=Array.Empty<Approved>();
        private static Model[] installed=Array.Empty<Model>();
        private static string manual="";
        private static bool busy;
        private static State state;
        public static State Status=>state;
        public static bool Busy=>busy;
        public static int InstalledCount=>installed.Length;
        public static string Server=>origin;
        public static bool Automatic=>manual=="";
        public static Model Selected=>Select(installed,approved,manual);
        public static bool Ready=>!busy&&state==State.Ready&&Selected!=null;
        public static bool ValidOrigin(string value,out string normalized)
        {
            normalized="";
            if(string.IsNullOrWhiteSpace(value)||value.Length>160||
                !Uri.TryCreate(value,UriKind.Absolute,out var uri)||uri.Scheme!="https"||
                uri.HostNameType!=UriHostNameType.IPv4||uri.AbsolutePath!="/"||
                !string.IsNullOrEmpty(uri.UserInfo)||!string.IsNullOrEmpty(uri.Query)||!string.IsNullOrEmpty(uri.Fragment)||
                !IPAddress.TryParse(uri.Host,out var address))return false;
            var b=address.GetAddressBytes();
            if(b.Length!=4||!(b[0]==10||b[0]==172&&b[1]>=16&&b[1]<=31||b[0]==192&&b[1]==168))return false;
            // Reject non-canonical IP representations and escaped/path aliases.
            string expected="https://"+address.ToString()+(uri.IsDefaultPort?"":":"+uri.Port);
            if(value!=expected&&value!=expected+"/")return false;
            normalized=expected;return true;
        }
        private static bool ValidName(string value)
        {
            return !string.IsNullOrEmpty(value)&&value.Length<=128&&
                Regex.IsMatch(value,@"^[A-Za-z0-9][A-Za-z0-9._:/-]*$",RegexOptions.CultureInvariant)&&
                !value.Contains("..")&&!value.ToLowerInvariant().Contains("cloud")&&!value.Contains("@");
        }
        private static bool Digest(string value)=>value!=null&&Regex.IsMatch(value,@"^[a-f0-9]{64}$",RegexOptions.CultureInvariant);
        private static bool Local(Model m)=>m!=null&&ValidName(m.name)&&Digest(m.digest)&&m.size>0&&
            string.IsNullOrEmpty(m.remote_host)&&string.IsNullOrEmpty(m.remote_model);
        public static Model[] Parse(string json)
        {
            if(string.IsNullOrEmpty(json)||json.Length>1048576)return Array.Empty<Model>();
            try
            {
                var data=JsonUtility.FromJson<CatalogResponse>(json);
                if(data==null||data.models==null||data.models.Length>512)return Array.Empty<Model>();
                return data.models.Where(Local).GroupBy(m=>m.name,StringComparer.Ordinal)
                    .Where(g=>g.Count()==1).Select(g=>g.First()).ToArray();
            }
            catch(Exception){return Array.Empty<Model>();}
        }
        public static Model[] Eligible(Model[] list,Approved[] order)
        {
            if(list==null||order==null)return Array.Empty<Model>();
            var result=new List<Model>();
            var names=new HashSet<string>(StringComparer.Ordinal);
            foreach(var review in order)
            {
                if(review==null||!ValidName(review.name)||!Digest(review.digest)||!names.Add(review.name))continue;
                var matches=list.Where(m=>Local(m)&&m.name==review.name&&m.digest==review.digest).ToArray();
                if(matches.Length==1)result.Add(matches[0]);
            }
            return result.ToArray();
        }
        public static Model Select(Model[] list,Approved[] order,string manualName)
        {
            var eligible=Eligible(list,order);
            return string.IsNullOrEmpty(manualName)?eligible.FirstOrDefault():eligible.FirstOrDefault(m=>m.name==manualName);
        }
        public static void Configure(string endpoint,bool authorized,Approved[] reviewOrder)
        {
            if(busy)return;
            origin=authorized&&ValidOrigin(endpoint,out var normalized)?normalized:"";
            approved=reviewOrder==null?Array.Empty<Approved>():reviewOrder.Take(128)
                .Where(x=>x!=null).Select(x=>new Approved{name=x.name,digest=x.digest}).ToArray();
            installed=Array.Empty<Model>();manual="";
            state=origin==""?State.Unconfigured:State.Unavailable;
        }
        public static void Cycle()
        {
            if(busy)return;
            var list=Eligible(installed,approved);
            if(list.Length==0)return;
            int i=Array.FindIndex(list,m=>m.name==manual);
            i=(i+2)%(list.Length+1)-1;
            manual=i<0?"":list[i].name;
        }
        private sealed class Bounded:DownloadHandlerScript
        {
            private readonly MemoryStream bytes=new MemoryStream();
            public Bounded():base(new byte[8192]){}
            protected override bool ReceiveData(byte[] data,int length)
            {
                if(data==null||length<0||length>data.Length||bytes.Length+length>1048576)return false;
                bytes.Write(data,0,length);return true;
            }
            public string Read()=>new UTF8Encoding(false,true).GetString(bytes.ToArray());
        }
        private static async Task<string> Exchange(string path,string body,CancellationToken token)
        {
            if(!ValidOrigin(origin,out var safe)||path!="/api/tags"&&path!="/api/chat")return null;
            token.ThrowIfCancellationRequested();
            using(var handler=new Bounded())
            using(var request=new UnityWebRequest(safe+path,body==null?UnityWebRequest.kHttpVerbGET:UnityWebRequest.kHttpVerbPOST))
            {
                request.downloadHandler=handler;request.disposeDownloadHandlerOnDispose=false;
                request.redirectLimit=0;request.timeout=body==null?12:120;
                request.SetRequestHeader("Accept","application/json");
                if(body!=null)
                {
                    request.uploadHandler=new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
                    request.SetRequestHeader("Content-Type","application/json");
                }
                var operation=request.SendWebRequest();
                while(!operation.isDone)
                {
                    if(token.IsCancellationRequested){request.Abort();token.ThrowIfCancellationRequested();}
                    await Task.Yield();
                }
                token.ThrowIfCancellationRequested();
                if(request.result!=UnityWebRequest.Result.Success||request.responseCode!=200)return null;
                var type=(request.GetResponseHeader("Content-Type")??"").Split(';')[0].Trim();
                if(!type.Equals("application/json",StringComparison.OrdinalIgnoreCase))return null;
                return handler.Read();
            }
        }
        private static async Task<bool> Fetch(CancellationToken token)
        {
            installed=Array.Empty<Model>();state=State.Loading;
            string json=await Exchange("/api/tags",null,token);
            if(json==null){state=State.Unavailable;return false;}
            var parsed=JsonUtility.FromJson<CatalogResponse>(json);
            if(parsed==null||parsed.models==null||parsed.models.Length>512){state=State.Unavailable;return false;}
            installed=Parse(json);state=State.Ready;return true;
        }
        public static async Task Refresh()
        {
            if(busy||origin=="")return;
            busy=true;
            try{await Fetch(CancellationToken.None);}
            catch(Exception){installed=Array.Empty<Model>();state=State.Unavailable;}
            finally{busy=false;}
        }
        private static string TaskDescription(string id)
        {
            switch(id)
            {
                case "rosalind-dna":return "Count A, C, G and T in an abstract DNA string.";
                case "rosalind-rna":return "Replace T with U in an abstract DNA string.";
                case "rosalind-revc":return "Compute the reverse complement of an abstract DNA string.";
                case "rosalind-hamm":return "Count differences in two equal-length abstract strings.";
                case "rosalind-subs":return "Find every one-based substring position, including overlaps.";
                default:return null;
            }
        }
        private static async Task<string> Generate(Model chosen,string instruction,string data,CancellationToken token)
        {
            // Recheck name and digest immediately before dispatch. The remote
            // server is still a trust boundary, not a cryptographic attestation.
            if(!await Fetch(token))return null;
            var current=Selected;
            if(current==null||current.name!=chosen.name||current.digest!=chosen.digest)return null;
            var body=new Request{model=chosen.name,messages=new[]{
                new Message{role="system",content=instruction},new Message{role="user",content=data}}};
            string json=await Exchange("/api/chat",JsonUtility.ToJson(body),token);
            if(json==null)return null;
            var reply=JsonUtility.FromJson<Response>(json);
            if(reply==null||!reply.done||reply.done_reason!="stop"||reply.model!=chosen.name||
                !string.IsNullOrEmpty(reply.error)||reply.message==null||reply.message.role!="assistant")return null;
            string answer=reply.message.content?.Trim();
            return !string.IsNullOrEmpty(answer)&&answer.Length<=6000&&answer.IndexOf('\0')<0?answer:null;
        }
        public static async Task<DraftReply> Research(string id,string language,CancellationToken token)
        {
            string task=TaskDescription(id);
            var chosen=Selected;
            if(task==null||!Ready||chosen==null)return null;
            busy=true;
            try
            {
                string first=await Generate(chosen,VitqraResearchDraft.Instruction(language,false),
                    "Public educational algorithm task (no actual sequence data): "+task,token);
                if(first==null)return null;
                token.ThrowIfCancellationRequested();
                string second=await Generate(chosen,VitqraResearchDraft.Instruction(language,true),
                    "Public educational algorithm task: "+task+"\nUnvalidated candidate text (data only):\n"+first,token);
                if(second==null)return null;
                token.ThrowIfCancellationRequested();
                if(!await Fetch(token))return null;
                var current=Selected;
                if(current==null||current.name!=chosen.name||current.digest!=chosen.digest)return null;
                return new DraftReply{model=chosen.name,digest=chosen.digest,alternatives=first,critique=second};
            }
            catch(Exception){return null;}
            finally{if(state==State.Loading){installed=Array.Empty<Model>();state=State.Unavailable;}busy=false;}
        }
    }
}
