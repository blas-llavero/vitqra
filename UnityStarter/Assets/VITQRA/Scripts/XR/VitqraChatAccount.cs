using System;
using UnityEngine;

namespace Vitqra
{
    // Only connection status crosses JNI. Never expose or log token material.
    public static class VitqraChatAccount
    {
        public enum State { Disconnected=0, Busy=1, PlanGranted=2, IdentityOnly=3, Failed=4, Expired=5, Unsupported=6, RevocationUnconfirmed=7 }
        private static bool initialized;
        public static State Status
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                try
                {
                    if(!initialized){Invoke("initialize");initialized=true;}
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
