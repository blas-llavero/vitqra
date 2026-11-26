using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using D=Vitqra.VitqraDiscoveryStrings.Key;

namespace Vitqra
{
    public sealed partial class VitqraDiscoveryPanel
    {
        private readonly Dictionary<string,D> evidenceStates=new Dictionary<string,D>(StringComparer.Ordinal);
        private VitqraChallengeDirectory.Item manualEvidence;
        private UnityWebRequest evidenceRequest;
        private Coroutine evidenceRoutine;
        private bool evidenceBusy,evidencePaused;
        private int evidenceJobs;
        private float evidenceNext;
        private string EvidenceRoot=>Path.Combine(Application.persistentDataPath,"VITQRA","ResearchEvidence");
        private static string EvidenceKey(VitqraChallengeDirectory.Item item)=>item.id+"\n"+item.url;
        private string EvidenceSuffix(VitqraChallengeDirectory.Item item)
        {
            return evidenceStates.TryGetValue(EvidenceKey(item),out var state)?" · "+T(state):"";
        }
        private void QueueManualEvidence(VitqraChallengeDirectory.Item item)
        {
            if(VitqraEvidence.Eligible(item)&&!evidenceStates.ContainsKey(EvidenceKey(item)))manualEvidence=item;
        }
        private void UpdateEvidence()
        {
            if(evidencePaused||evidenceBusy||Time.unscaledTime<evidenceNext||evidenceJobs>=VitqraEvidence.MaxJobs)return;
            VitqraChallengeDirectory.Item item=null;
            if(automatic)
            {
                manualEvidence=null;
                foreach(var candidate in filtered)
                    if(VitqraEvidence.Eligible(candidate)&&!evidenceStates.ContainsKey(EvidenceKey(candidate)))
                    {item=candidate;break;}
            }
            else if(manualEvidence!=null){item=manualEvidence;manualEvidence=null;}
            if(item==null||evidenceStates.ContainsKey(EvidenceKey(item)))return;
            evidenceBusy=true;evidenceJobs++;
            evidenceStates[EvidenceKey(item)]=D.EvidenceLoading;
            Render();
            evidenceRoutine=StartCoroutine(AcquireEvidence(item));
        }
        private IEnumerator AcquireEvidence(VitqraChallengeDirectory.Item item)
        {
            VitqraEvidence.Result result=null;
            yield return VitqraEvidence.Acquire(item,EvidenceRoot,request=>evidenceRequest=request,value=>result=value);
            evidenceStates[EvidenceKey(item)]=result!=null&&result.success?
                (result.partial?D.EvidencePartial:D.EvidenceSaved):D.EvidenceFailed;
            evidenceBusy=false;evidenceRoutine=null;evidenceNext=Time.unscaledTime+2f;Render();
        }
        private void CancelEvidence()
        {
            if(evidenceRequest!=null)evidenceRequest.Abort();
            if(evidenceRoutine!=null)StopCoroutine(evidenceRoutine);
            evidenceRequest=null;evidenceRoutine=null;evidenceBusy=false;manualEvidence=null;
            var canceled=new List<string>();
            foreach(var pair in evidenceStates)if(pair.Value==D.EvidenceLoading)canceled.Add(pair.Key);
            foreach(var key in canceled)evidenceStates.Remove(key);
            evidenceNext=Time.unscaledTime+2f;
        }
        private void OnApplicationPause(bool paused)
        {
            evidencePaused=paused;
            if(paused)CancelEvidence();
            Render();
        }
    }
}
