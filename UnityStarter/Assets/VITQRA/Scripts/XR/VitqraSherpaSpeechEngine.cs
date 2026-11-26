using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Vitqra
{
    // Assign only reviewed local model files; no URLs, remote endpoints or downloads.
    public sealed class VitqraSherpaSpeechEngine : VitqraOfflineSpeechEngine
    {
        [Serializable]
        public sealed class ReviewedFile
        {
            public string relativePath;
            public string sha256;
        }
        [Serializable]
        public sealed class VoiceProfile
        {
            public string language = "en";
            [Range(0,3)] public int avatarProfile;
            public int speakerId;
            public ReviewedFile model, lexicon, tokens;
        }
        [SerializeField] private ReviewedFile whisperEncoder, whisperDecoder, whisperTokens;
        [SerializeField] private VoiceProfile[] voices = new VoiceProfile[0];
        [SerializeField] private string[] recognitionLanguages = {"en","es","ca","zh","hi","fr","de","pt","ar","ja"};
        private string modelRoot;
        private bool nativeReady, recognitionReady;
        private readonly System.Collections.Generic.List<VoiceProfile> readyVoices = new System.Collections.Generic.List<VoiceProfile>();
        private CancellationTokenSource lifetime;
        private static readonly SemaphoreSlim NativeGate = new SemaphoreSlim(1,1);

        [DllImport("vitqra_speech",CallingConvention=CallingConvention.Cdecl)]
        private static extern int vitqra_speech_abi();
        [DllImport("vitqra_speech",CallingConvention=CallingConvention.Cdecl)]
        private static extern int vitqra_transcribe(byte[] encoder,byte[] decoder,byte[] tokens,byte[] language,
            [In] float[] pcm,int count,int rate,[Out] byte[] output,int capacity);
        [DllImport("vitqra_speech",CallingConvention=CallingConvention.Cdecl)]
        private static extern int vitqra_synthesize(byte[] model,byte[] lexicon,byte[] tokens,byte[] text,int speaker,
            float speed,[Out] float[] output,int capacity,out int sampleRate);

        private async void Awake()
        {
            modelRoot=Path.GetFullPath(Path.Combine(Application.persistentDataPath,"speech-models"));
            lifetime=new CancellationTokenSource();
            var token=lifetime.Token;
            try
            {
                nativeReady=vitqra_speech_abi()==1;
                if(!nativeReady)return;
                recognitionReady=await Task.Run(()=>Verify(whisperEncoder,token)&&Verify(whisperDecoder,token)&&Verify(whisperTokens,token),token);
                if(token.IsCancellationRequested)return;
                foreach(var profile in voices ?? new VoiceProfile[0])
                {
                    if(profile==null||profile.speakerId<0||profile.avatarProfile<0||profile.avatarProfile>3)continue;
                    bool valid=await Task.Run(()=>Verify(profile.model,token)&&Verify(profile.lexicon,token)&&Verify(profile.tokens,token),token);
                    if(token.IsCancellationRequested)return;
                    if(valid)readyVoices.Add(profile);
                }
            }
            catch(Exception){recognitionReady=false;nativeReady=false;readyVoices.Clear();}
        }
        private string Resolve(ReviewedFile file)
        {
            if(file==null||string.IsNullOrWhiteSpace(file.relativePath)||Path.IsPathRooted(file.relativePath))
                throw new IOException("Invalid model path");
            string full=Path.GetFullPath(Path.Combine(modelRoot,file.relativePath));
            if(!full.StartsWith(modelRoot+Path.DirectorySeparatorChar,StringComparison.Ordinal))
                throw new IOException("Model path outside private directory");
            return full;
        }
        private bool Verify(ReviewedFile file,CancellationToken token)
        {
            try
            {
                if(file==null||string.IsNullOrEmpty(file.sha256)||file.sha256.Length!=64)return false;
                token.ThrowIfCancellationRequested();
                using(var sha=SHA256.Create())
                using(var stream=File.OpenRead(Resolve(file)))
                {
                    string actual=BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","");
                    token.ThrowIfCancellationRequested();
                    return string.Equals(actual,file.sha256,StringComparison.OrdinalIgnoreCase);
                }
            }
            catch(OperationCanceledException){throw;}
            catch(Exception){return false;}
        }
        private static byte[] Utf8(string value)
        {
            if(value==null||value.IndexOf('\0')>=0)throw new ArgumentException("Invalid text");
            return Encoding.UTF8.GetBytes(value+"\0");
        }
        public override bool IsReady(string language)
        {
            return nativeReady&&recognitionReady&&Array.IndexOf(recognitionLanguages,language)>=0;
        }
        public override async Task<string> TranscribeAsync(float[] monoPcm,int sampleRate,string language,CancellationToken token)
        {
            if(!IsReady(language))throw new InvalidOperationException("Offline ASR not ready");
            var encoder=Utf8(Resolve(whisperEncoder));
            var decoder=Utf8(Resolve(whisperDecoder));
            var tokens=Utf8(Resolve(whisperTokens));
            var lang=Utf8(language);
            await NativeGate.WaitAsync(token);
            try
            {
                return await Task.Run(()=>{
                    token.ThrowIfCancellationRequested();
                    // Recheck file identity before inference; install/update only while stopped.
                    if(!Verify(whisperEncoder,token)||!Verify(whisperDecoder,token)||!Verify(whisperTokens,token))
                        throw new IOException("Model verification failed");
                    var output=new byte[65536];
                    try
                    {
                        int n=vitqra_transcribe(encoder,decoder,tokens,lang,monoPcm,monoPcm.Length,sampleRate,output,output.Length);
                        token.ThrowIfCancellationRequested();
                        if(n<0||n>=output.Length)throw new InvalidOperationException("Offline ASR failed");
                        return Encoding.UTF8.GetString(output,0,n);
                    }
                    finally{Array.Clear(output,0,output.Length);}
                },token);
            }
            finally{NativeGate.Release();}
        }
        public sealed class SpeechAudio
        {
            public float[] samples;
            public int sampleRate;
        }
        public bool CanSpeak(string language,int avatar)
        {
            return nativeReady&&readyVoices.Exists(v=>v.language==language&&v.avatarProfile==avatar);
        }
        public async Task<SpeechAudio> SynthesizeAsync(string text,string language,int avatar,float speed,CancellationToken token)
        {
            var voice=readyVoices.Find(v=>v.language==language&&v.avatarProfile==avatar);
            if(!nativeReady||voice==null)throw new InvalidOperationException("Offline voice not ready");
            if(string.IsNullOrWhiteSpace(text)||text.Length>400)throw new ArgumentException("Voice text limit");
            var model=Utf8(Resolve(voice.model));var lexicon=Utf8(Resolve(voice.lexicon));var tokens=Utf8(Resolve(voice.tokens));
            var input=Utf8(text);
            await NativeGate.WaitAsync(token);
            try
            {
                return await Task.Run(()=>{
                    token.ThrowIfCancellationRequested();
                    if(!Verify(voice.model,token)||!Verify(voice.lexicon,token)||!Verify(voice.tokens,token))
                        throw new IOException("Voice model verification failed");
                    var buffer=new float[60*48000];
                    try
                    {
                        int n=vitqra_synthesize(model,lexicon,tokens,input,voice.speakerId,speed,buffer,buffer.Length,out int rate);
                        token.ThrowIfCancellationRequested();
                        if(n<=0||n>buffer.Length||rate<8000||rate>48000)throw new InvalidOperationException("Offline TTS failed");
                        var audio=new float[n];Array.Copy(buffer,audio,n);
                        return new SpeechAudio {samples=audio,sampleRate=rate};
                    }
                    finally{Array.Clear(buffer,0,buffer.Length);}
                },token);
            }
            finally{Array.Clear(input,0,input.Length);NativeGate.Release();}
        }
        private void OnDestroy()
        {
            if(lifetime!=null){lifetime.Cancel();lifetime.Dispose();lifetime=null;}
        }
    }
}
