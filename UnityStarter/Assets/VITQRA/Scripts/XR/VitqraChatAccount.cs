using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Vitqra
{
    // Only connection status crosses JNI. Never expose or log token material.
    public static class VitqraChatAccount
    {
        public enum State { Disconnected=0, Busy=1, PlanGranted=2, IdentityOnly=3, Failed=4, Expired=5, Unsupported=6, RevocationUnconfirmed=7 }
        private static bool initialized;
        public static void InitializeCatalog()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if(initialized)return;
            try{Invoke("initialize");initialized=true;}catch(Exception){}
#endif
        }
        public static State Status
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                try
                {
                    InitializeCatalog();
                    using(var api=new AndroidJavaClass("com.blasllavero.vitqra.account.VitqraChatAccount"))
                    {
                        int state=api.CallStatic<int>("getStatus");
                        return state>=0&&state<=7?(State)state:State.Failed;
                    }
                }
                catch(Exception){return State.Failed;}
#else
                return State.Unsupported;
#endif
            }
        }
        private static void Invoke(string method)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using(var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using(var activity=player.GetStatic<AndroidJavaObject>("currentActivity"))
            using(var api=new AndroidJavaClass("com.blasllavero.vitqra.account.VitqraChatAccount"))
                api.CallStatic(method,activity);
#endif
        }
        [Serializable] public sealed class Model { public string slug,name; }
        [Serializable] private sealed class Models { public Model[] models; }
        private static T Read<T>(string method,T fallback,bool withActivity=false)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using(var api=new AndroidJavaClass("com.blasllavero.vitqra.account.VitqraChatAccount"))
                {
                    if(!withActivity)return api.CallStatic<T>(method);
                    using(var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    using(var activity=player.GetStatic<AndroidJavaObject>("currentActivity"))
                        return api.CallStatic<T>(method,activity);
                }
            }
            catch(Exception){}
#endif
            return fallback;
        }
        public static Model[] Catalog
        {
            get
            {
                try{return JsonUtility.FromJson<Models>("{\"models\":"+Read("getCatalog","[]")+"}").models??Array.Empty<Model>();}
                catch(Exception){return Array.Empty<Model>();}
            }
        }
        public static int CatalogState=>Read("getCatalogState",0);
        public static int TestState=>Read("getTestState",0);
        public static int DailyLimit=>Read("getLimit",40,true);
        public static int Used=>Read("getUsed",0,true);
        public static void RefreshCatalog(){try{Invoke("refreshCatalog");}catch(Exception){}}
        public static void CycleLimit(){try{Invoke("cycleLimit");}catch(Exception){}}
        public static void TestConnection(string slug)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using(var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using(var activity=player.GetStatic<AndroidJavaObject>("currentActivity"))
                using(var api=new AndroidJavaClass("com.blasllavero.vitqra.account.VitqraChatAccount"))
                    api.CallStatic("testConnection",activity,slug);
            }
            catch(Exception){}
#endif
        }
        // Model preference is shared by the settings panel and research queue.
        // The manual choice remains session-only; the default is automatic.
        public static string ManualModel {get;set;}="";
        public static string ResearchModel=>CatalogState==2?VitqraResearchModelSelector.Resolve(Catalog,ManualModel):"";
        public static bool ResearchConsent=>Read("getResearchConsent",false);
        public static bool ResearchReady=>ResearchConsent&&Status==State.PlanGranted&&CatalogState==2&&
            !string.IsNullOrEmpty(ResearchModel)&&DailyLimit-Used>=2;
        public static void SetResearchConsent(bool enabled)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try{using(var api=new AndroidJavaClass("com.blasllavero.vitqra.account.VitqraChatAccount"))api.CallStatic("setResearchConsent",enabled);}
            catch(Exception){}
#endif
        }
        public static void CancelResearch()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try{using(var api=new AndroidJavaClass("com.blasllavero.vitqra.account.VitqraChatAccount"))api.CallStatic("cancelResearch");}
            catch(Exception){}
#endif
        }
        [Serializable] public sealed class ResearchReply{public string model,alternatives,critique;}
        public static async Task<ResearchReply> Research(string slug,string id,string language,CancellationToken token,VitqraMotifBenchmark.Trial training=null)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                token.ThrowIfCancellationRequested();
                using(var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using(var activity=player.GetStatic<AndroidJavaObject>("currentActivity"))
                using(var api=new AndroidJavaClass("com.blasllavero.vitqra.account.VitqraChatAccount"))
                    if(!api.CallStatic<bool>("startResearch",activity,slug,id,language,training==null?0:training.baselineComparisons,training==null?0:training.kmpComparisons))return null;
                bool canceled=false;
                while(Read("getResearchState",3)==1)
                {
                    if(token.IsCancellationRequested&&!canceled){CancelResearch();canceled=true;}
                    await Task.Delay(200);
                }
                token.ThrowIfCancellationRequested();
                if(Read("getResearchState",3)!=2||!ResearchConsent)return null;
                var result=JsonUtility.FromJson<ResearchReply>(Read("getResearchResult",""));
                CancelResearch(); // clear result from the Java bridge
                return result;
            }
            catch(Exception){CancelResearch();return null;}
#else
            await Task.Yield();return null;
#endif
        }
        public static void SignIn(){try{Invoke("signIn");}catch(Exception){}}
        public static void SignOut(){try{Invoke("signOut");}catch(Exception){}}
        public static void Cancel()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try{using(var api=new AndroidJavaClass("com.blasllavero.vitqra.account.VitqraChatAccount"))api.CallStatic("cancel");}
            catch(Exception){}
#endif
        }
    }
}
