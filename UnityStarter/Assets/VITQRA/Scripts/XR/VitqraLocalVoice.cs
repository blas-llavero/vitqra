using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Vitqra
{
    // A reviewed, offline native engine must implement this contract.
    // No engine, remote endpoint, model binary or fallback is bundled.
    public abstract class VitqraOfflineSpeechEngine : MonoBehaviour
    {
        public abstract bool IsReady(string language);
        public abstract Task<string> TranscribeAsync(float[] monoPcm,int sampleRate,string language,CancellationToken token);
    }

    public sealed class VitqraLocalVoice : MonoBehaviour
    {
        [SerializeField] private VitqraOfflineSpeechEngine engine;
        private VitqraConversationPanel panel;
        private string language, device;
        private AudioClip clip;
        private float began;
        private CancellationTokenSource pending;
        private const int Rate=16000;
        private const int MaxSeconds=20;
        private bool configured;
        public void Configure(VitqraConversationPanel conversation,string locale)
        {
            if(configured) return;
            panel=conversation;
            language=locale;
            configured=true;
            // Do not attach the microphone button until a concrete engine is supplied.
            if(engine==null) engine=GetComponent<VitqraOfflineSpeechEngine>();
            if(engine==null) return;
            panel.VoiceRequested+=Toggle;
            panel.VoiceCanceled+=Cancel;
        }

        private void Toggle()
        {
            if(clip!=null){Finish();return;}
            if(pending!=null){Cancel();return;}
            if(engine==null||!engine.IsReady(language)){panel.SetVoiceState(false,false,true);return;}
#if UNITY_ANDROID && !UNITY_EDITOR
            if(!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone))
            {
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
                // A second explicit press is required after permission is granted.
                panel.SetVoiceState(false,false,true);
                return;
            }
#endif
            var playback=GetComponent<VitqraSpeechPlayback>();
            if(playback!=null)playback.Stop();
            try
            {
                if(Microphone.devices.Length==0){panel.SetVoiceState(false,false,true);return;}
                device=Microphone.devices[0];
                clip=Microphone.Start(device,false,MaxSeconds+1,Rate);
                if(clip==null){Microphone.End(device);panel.SetVoiceState(false,false,true);return;}
                began=Time.realtimeSinceStartup;
                panel.SetVoiceState(true);
            }
            catch(Exception){Cancel();panel.SetVoiceState(false,false,true);}
        }

        private void Update()
        {
            if(clip!=null && Time.realtimeSinceStartup-began>=MaxSeconds) Finish();
        }

        private async void Finish()
        {
            AudioClip recording=clip;
            if(recording==null) return;
            clip=null;
            float[] interleaved=null, mono=null;
            CancellationTokenSource operation=null;
            try
            {
                int frames=Microphone.GetPosition(device);
                Microphone.End(device);
                if(frames<=0) throw new InvalidOperationException();
                frames=Math.Min(frames,recording.samples);
                int channels=recording.channels;
                interleaved=new float[frames*channels];
                if(!recording.GetData(interleaved,0)) throw new InvalidOperationException();
                mono=new float[frames];
                for(int i=0;i<frames;i++)
                {
                    for(int c=0;c<channels;c++) mono[i]+=interleaved[i*channels+c];
                    mono[i]/=channels;
                }
                operation=new CancellationTokenSource();
                pending=operation;
                panel.SetVoiceState(false,true);
                var text=await engine.TranscribeAsync(mono,recording.frequency,language,operation.Token);
                if(!operation.IsCancellationRequested&&panel!=null&&panel.gameObject.activeInHierarchy)
                {
                    panel.ApplyVoiceTranscript(text);
                    panel.SetVoiceState(false);
                }
            }
            catch(Exception)
            {
                if((operation==null||!operation.IsCancellationRequested)&&panel!=null) panel.SetVoiceState(false,false,true);
            }
            finally
            {
                if(interleaved!=null) Array.Clear(interleaved,0,interleaved.Length);
                if(mono!=null) Array.Clear(mono,0,mono.Length);
                if(recording!=null) Destroy(recording);
                if(operation!=null)
                {
                    if(ReferenceEquals(pending,operation)) pending=null;
                    operation.Dispose();
                }
            }
        }

        private void Cancel()
        {
            if(clip!=null)
            {
                Microphone.End(device);
                Destroy(clip);
                clip=null;
            }
            if(pending!=null) pending.Cancel();
            if(panel!=null) panel.SetVoiceState(false);
        }
        private void OnApplicationPause(bool paused){if(paused) Cancel();}
        private void OnDisable(){Cancel();}
        private void OnDestroy()
        {
            if(panel!=null){panel.VoiceRequested-=Toggle;panel.VoiceCanceled-=Cancel;}
        }
    }
}
