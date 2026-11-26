using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using C=Vitqra.VitqraChatStrings.Key;

namespace Vitqra
{
    // Optional native component. All configuration is local. No endpoint or
    // commercial provider exists. Conversation and bounded public research drafts use isolated requests.
    public sealed class VitqraLocalAi : MonoBehaviour
    {
        [SerializeField] private bool enableReviewedModel;
        [SerializeField] private string reviewedModelSha256="";
        [SerializeField] private string modelLicenseSpdx="";
        [SerializeField] private string modelProvenance="";
        private const long MaxModelBytes=2L*1024*1024*1024;
        private static readonly SemaphoreSlim Gate=new SemaphoreSlim(1,1);
        private VitqraConversationPanel panel;
        private Action<string> deliver;
        private string modelPath,locale;
        private string previousUser="",previousAssistant="";
        private CancellationTokenSource operation;
        private IntPtr job;
        private float startedAt;
        private bool destroying;
        public bool Configured=>enableReviewedModel&&ValidHash(reviewedModelSha256)&&
            !string.IsNullOrWhiteSpace(modelLicenseSpdx)&&!string.IsNullOrWhiteSpace(modelProvenance)&&
            !string.IsNullOrEmpty(modelPath)&&File.Exists(modelPath);
        [DllImport("vitqra_llm",CallingConvention=CallingConvention.Cdecl)] private static extern int vitqra_llm_abi();
        [DllImport("vitqra_llm",CallingConvention=CallingConvention.Cdecl)] private static extern IntPtr vitqra_llm_create();
        [DllImport("vitqra_llm",CallingConvention=CallingConvention.Cdecl)] private static extern void vitqra_llm_cancel(IntPtr handle);
        [DllImport("vitqra_llm",CallingConvention=CallingConvention.Cdecl)] private static extern void vitqra_llm_destroy(IntPtr handle);
        [DllImport("vitqra_llm",CallingConvention=CallingConvention.Cdecl)] private static extern int vitqra_llm_generate(
            IntPtr handle,byte[] path,byte[] system,byte[] priorUser,byte[] priorAssistant,byte[] user,
            [Out] byte[] output,int capacity,out int truncated);

        public void Configure(VitqraConversationPanel conversation,string language,Action<string> onResponse)
        {
            if(panel!=null)throw new InvalidOperationException("AI already configured");
            panel=conversation;locale=VitqraStrings.ResolveLanguage(language);deliver=onResponse;
            modelPath=Path.Combine(Application.persistentDataPath,"VITQRA","Models","model.gguf");
            panel.MessageSubmitted+=Submit;
            panel.AiCancelRequested+=Cancel;
            panel.HistoryCleared+=ClearContext;
        }
        public static bool ValidHash(string value)
        {
            if(string.IsNullOrEmpty(value)||value.Length!=64)return false;
            foreach(char c in value)if(!Uri.IsHexDigit(c))return false;
            return true;
        }
        public static string SystemPrompt(string locale)
        {
            string language;
            switch(VitqraStrings.NormalizeLocale(locale))
            {
                case "ca":language="Catalan";break;case "es":language="Spanish";break;
                case "zh":language="Simplified Chinese";break;case "hi":language="Hindi";break;
                case "fr":language="French";break;case "de":language="German";break;
                case "pt":language="Portuguese";break;case "ar":language="Arabic";break;
                case "ja":language="Japanese";break;default:language="English";break;
            }
            return "You are VITQRA, a research assistant. Reply in "+language+
                ". Distinguish evidence from hypotheses. Do not invent sources or claim to have browsed, run code, verified research or performed experiments. "+
                "State uncertainty. Do not give patient-specific treatment advice. You have no tools and cannot perform external actions.";
        }
        private static byte[] Utf8(string value)
        {
            if(value.IndexOf('\0')>=0)throw new ArgumentException("Embedded NUL");
            return new UTF8Encoding(false,true).GetBytes(value+"\0");
        }
        private static string Bound(string value,int max)
        {
            if(value.Length<=max)return value;
            if(char.IsHighSurrogate(value[max-1]))max--;
            return value.Substring(0,max);
        }
        private static bool Verify(string path,string expected,CancellationToken token)
        {
            if(!ValidHash(expected))return false;
            using(var file=File.OpenRead(path))
            using(var hash=SHA256.Create())
            {
                if(file.Length<4||file.Length>MaxModelBytes)return false;
                byte[] magic=new byte[4];
                if(file.Read(magic,0,4)!=4||magic[0]!=71||magic[1]!=71||magic[2]!=85||magic[3]!=70)return false;
                file.Position=0;
                byte[] buffer=new byte[1048576];
                int n;
                while((n=file.Read(buffer,0,buffer.Length))>0)
                {
                    token.ThrowIfCancellationRequested();
                    hash.TransformBlock(buffer,0,n,buffer,0);
                }
                hash.TransformFinalBlock(Array.Empty<byte>(),0,0);
                return string.Equals(BitConverter.ToString(hash.Hash).Replace("-",""),expected,StringComparison.OrdinalIgnoreCase);
            }
        }
        private async void Submit(string message)
        {
            if(operation!=null||destroying||!isActiveAndEnabled)return;
            if(!Configured){panel.SetAiState(false,C.AiNotConfigured);return;}
            var op=new CancellationTokenSource();
            operation=op;startedAt=Time.unscaledTime;
            panel.SetAiState(true,C.AiGenerating);
            bool acquired=false;
            IntPtr handle=IntPtr.Zero;
            try
            {
                await Gate.WaitAsync(op.Token);acquired=true;
                op.Token.ThrowIfCancellationRequested();
                if(vitqra_llm_abi()!=1)throw new InvalidOperationException("ABI mismatch");
                handle=vitqra_llm_create();job=handle;
                if(handle==IntPtr.Zero)throw new InvalidOperationException("No job");
                string path=modelPath,expected=reviewedModelSha256,system=SystemPrompt(locale);
                string priorU=previousUser,priorA=previousAssistant;
                var response=await Task.Run(()=>{
                    if(!Verify(path,expected,op.Token))throw new InvalidDataException("Model verification failed");
                    op.Token.ThrowIfCancellationRequested();
                    byte[] input=Utf8(message),oldU=Utf8(priorU),oldA=Utf8(priorA),output=new byte[32768];
                    try
                    {
                        int count=vitqra_llm_generate(handle,Utf8(path),Utf8(system),oldU,oldA,input,output,output.Length,out int truncated);
                        op.Token.ThrowIfCancellationRequested();
                        if(count<=0||count>=output.Length)throw new InvalidOperationException("Inference failed");
                        // A token limit can end in an incomplete UTF-8 character.
                        // Reject malformed output instead of guessing at the contents.
                        string value=new UTF8Encoding(false,true).GetString(output,0,count).Trim();
                        if(string.IsNullOrWhiteSpace(value))throw new InvalidOperationException("Empty answer");
                        return new Reply {text=Bound(value,7999),truncated=truncated!=0||value.Length>7999};
                    }
                    finally{Array.Clear(input,0,input.Length);Array.Clear(oldU,0,oldU.Length);Array.Clear(oldA,0,oldA.Length);Array.Clear(output,0,output.Length);}
                },op.Token);
                if(!op.IsCancellationRequested&&!destroying&&panel!=null&&panel.gameObject.activeInHierarchy)
                {
                    previousUser=Bound(message,1000);previousAssistant=Bound(response.text,1000);
                    deliver?.Invoke(response.text+(response.truncated?"…":""));
                    panel.SetAiState(false,response.truncated?C.AiTruncated:C.AiFinished);
                }
            }
            catch(OperationCanceledException){if(!destroying&&panel!=null)panel.SetAiState(false,C.AiCanceled);}
            catch(Exception){if(!destroying&&panel!=null)panel.SetAiState(false,op.IsCancellationRequested?C.AiCanceled:C.AiFailed);}
            finally
            {
                job=IntPtr.Zero;
                if(handle!=IntPtr.Zero)vitqra_llm_destroy(handle);
                if(acquired)Gate.Release();
                if(ReferenceEquals(operation,op))operation=null;
                op.Dispose();
            }
        }
        public string ReviewedModelHash=>Configured?reviewedModelSha256.ToLowerInvariant():null;

        // Isolated research request: no conversation history, patient data or tools.
        // The caller owns cancellation. The same process-wide gate serializes inference.
        public async Task<string> DraftResearchAsync(string instruction,string source,CancellationToken cancellation)
        {
            if(!Configured||destroying||!isActiveAndEnabled||string.IsNullOrWhiteSpace(source)||
                source.Length>3200||string.IsNullOrWhiteSpace(instruction)||instruction.Length>2000)return null;
            string path=modelPath,hash=reviewedModelSha256;
            using(var timeout=CancellationTokenSource.CreateLinkedTokenSource(cancellation))
            {
                timeout.CancelAfter(TimeSpan.FromSeconds(120));
                bool acquired=false;
                try
                {
                    await Gate.WaitAsync(timeout.Token);acquired=true;
                    return await Task.Run(()=>{
                        timeout.Token.ThrowIfCancellationRequested();
                        if(!Verify(path,hash,timeout.Token)||vitqra_llm_abi()!=1)return null;
                        IntPtr handle=vitqra_llm_create();
                        if(handle==IntPtr.Zero)return null;
                        try
                        {
                            // Dispose registration before destroying native handle.
                            using(timeout.Token.Register(()=>vitqra_llm_cancel(handle)))
                            {
                                timeout.Token.ThrowIfCancellationRequested();
                                byte[] output=new byte[32768],input=Utf8(source),system=Utf8(instruction);
                                try
                                {
                                    int count=vitqra_llm_generate(handle,Utf8(path),system,Utf8(""),Utf8(""),input,output,output.Length,out int truncated);
                                    timeout.Token.ThrowIfCancellationRequested();
                                    // Incomplete output is not an accepted proposal.
                                    if(count<=0||count>=output.Length||truncated!=0)return null;
                                    string answer=new UTF8Encoding(false,true).GetString(output,0,count).Trim();
                                    return answer.Length>0&&answer.Length<=6000&&answer.IndexOf('\0')<0?answer:null;
                                }
                                finally{Array.Clear(output,0,output.Length);Array.Clear(input,0,input.Length);Array.Clear(system,0,system.Length);}
                            }
                        }
                        finally{vitqra_llm_destroy(handle);}
                    },timeout.Token);
                }
                catch(OperationCanceledException){return null;}
                catch(Exception){return null;}
                finally{if(acquired)Gate.Release();}
            }
        }

        private sealed class Reply {public string text;public bool truncated;}
        public void Cancel()
        {
            if(operation==null)return;
            operation.Cancel();
            if(job!=IntPtr.Zero)vitqra_llm_cancel(job);
            if(!destroying&&panel!=null)panel.SetAiState(true,C.AiStopping);
        }
        private void ClearContext(){Cancel();previousUser="";previousAssistant="";}
        private void Update(){if(operation!=null&&!operation.IsCancellationRequested&&Time.unscaledTime-startedAt>120f)Cancel();}
        private void OnApplicationPause(bool paused){if(paused)Cancel();}
        private void OnDisable(){Cancel();}
        private void OnDestroy()
        {
            destroying=true;Cancel();previousUser="";previousAssistant="";
            if(panel!=null){panel.MessageSubmitted-=Submit;panel.AiCancelRequested-=Cancel;panel.HistoryCleared-=ClearContext;}
        }
    }
}
