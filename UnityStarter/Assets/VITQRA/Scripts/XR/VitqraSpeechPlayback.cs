using System;
using System.Threading;
using UnityEngine;

namespace Vitqra
{
    public sealed class VitqraSpeechPlayback : MonoBehaviour
    {
        [SerializeField] private bool enabledByUser;
        [SerializeField,Range(0f,1f)] private float volume=.8f;
        [SerializeField,Range(.75f,1.25f)] private float speed=1f;
        private VitqraSherpaSpeechEngine engine;
        private AudioSource source;
        private AudioClip current;
        private CancellationTokenSource operation;
        public void Configure(VitqraSherpaSpeechEngine backend)
        {
            engine=backend;
            source=gameObject.AddComponent<AudioSource>();
            source.playOnAwake=false;
            source.spatialBlend=0;
        }
        public async void Speak(string message,string locale,int avatar)
        {
            Stop();
            if(!enabledByUser||engine==null||source==null||!engine.CanSpeak(locale,avatar)||
               string.IsNullOrWhiteSpace(message)||message.Length>400)return;
            var request=new CancellationTokenSource();
            operation=request;
            VitqraSherpaSpeechEngine.SpeechAudio audio=null;
            try
            {
                audio=await engine.SynthesizeAsync(message,locale,avatar,Mathf.Clamp(speed,.75f,1.25f),request.Token);
                if(request.IsCancellationRequested||this==null||source==null)return;
                current=AudioClip.Create("VITQRA local voice",audio.samples.Length,1,audio.sampleRate,false);
                if(!current.SetData(audio.samples,0)){Destroy(current);current=null;return;}
                source.clip=current;
                source.volume=Mathf.Clamp01(volume);
                source.Play();
            }
            catch(Exception){ /* Text remains visible; no remote fallback. */ }
            finally
            {
                if(audio!=null)Array.Clear(audio.samples,0,audio.samples.Length);
                if(ReferenceEquals(operation,request))operation=null;
                request.Dispose();
            }
        }
        public void Stop()
        {
            if(operation!=null)operation.Cancel();
            if(source!=null){source.Stop();source.clip=null;}
            if(current!=null){Destroy(current);current=null;}
        }
        private void Update(){if(current!=null&&source!=null&&!source.isPlaying)Stop();}
        private void OnApplicationPause(bool paused){if(paused)Stop();}
        private void OnDisable(){Stop();}
    }
}
