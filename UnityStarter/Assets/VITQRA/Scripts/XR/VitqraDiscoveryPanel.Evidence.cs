using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
        private bool evidenceBusy,evidencePaused,runPaused,runStopped;
        private const string RunPreference="vitqra.research.run.v1";
        private string ResultRoot=>Path.Combine(Application.persistentDataPath,"VITQRA","EducationalResults");
        private string ReportRoot=>Path.Combine(Application.persistentDataPath,"VITQRA","ResearchReports");
        private readonly Dictionary<string,bool> reportStates=new Dictionary<string,bool>(StringComparer.Ordinal);
        private readonly Dictionary<string,D> draftStates=new Dictionary<string,D>(StringComparer.Ordinal);
        private VitqraLocalAi researchAi;
        private CancellationTokenSource draftCancel;
        private Task<bool> activeDraft;
        private Task<bool> activePackage;
        private CancellationTokenSource packageCancel;
        private string PackageRoot=>Path.Combine(Application.persistentDataPath,"VITQRA","ResearchPackages");
        private readonly Dictionary<string,bool> packageStates=new Dictionary<string,bool>(StringComparer.Ordinal);
        private async Task<bool> SavePackage(VitqraChallengeDirectory.Item item)
        {
            var token=new CancellationTokenSource();packageCancel=token;
            try{return await VitqraResearchPackage.Save(PackageRoot,VitqraResearchPackage.Build(EvidenceRoot,ResultRoot,ReportRoot,DraftRoot,item,locale),token.Token);}
            finally{if(ReferenceEquals(packageCancel,token))packageCancel=null;token.Dispose();}
        }
        private string DraftRoot=>Path.Combine(Application.persistentDataPath,"VITQRA",localResearchSelected?"ResearchDrafts":ollamaResearchSelected?"OllamaResearchDrafts":"ChatGptResearchDrafts");
        private bool EligibleForProvider(VitqraChallengeDirectory.Item item)=>VitqraEvidence.Eligible(item)&&(localResearchSelected||VitqraResearchDraft.SupportsExternal(item));
        private bool localResearchSelected=true,ollamaResearchSelected;
        private bool ExternalReady=>ollamaResearchSelected?VitqraOllama.Ready:VitqraChatAccount.ResearchReady;
        public void ConfigureResearchAi(VitqraLocalAi ai,bool localSelected=true,bool ollamaSelected=false)
        {
            if(localResearchSelected!=localSelected||ollamaResearchSelected!=ollamaSelected){CancelEvidence();evidenceStates.Clear();draftStates.Clear();packageStates.Clear();reportStates.Clear();}
            localResearchSelected=localSelected;ollamaResearchSelected=ollamaSelected;
            researchAi=localSelected?ai:null;
        }
        private async Task<bool> GenerateDraft(VitqraChallengeDirectory.Item item)
        {
            var cancel=new CancellationTokenSource();
            draftCancel=cancel;
            try
            {
                return localResearchSelected?await VitqraResearchDraft.Generate(researchAi,EvidenceRoot,DraftRoot,item,locale,cancel.Token):
                    ollamaResearchSelected?await VitqraResearchDraft.GenerateOllama(EvidenceRoot,DraftRoot,item,locale,cancel.Token):
                    await VitqraResearchDraft.GenerateExternal(EvidenceRoot,DraftRoot,item,locale,cancel.Token);
            }
            finally
            {
                if(ReferenceEquals(draftCancel,cancel))draftCancel=null;
                cancel.Dispose();
            }
        }
        private int evidenceJobs;
        private float evidenceNext;
        private string EvidenceRoot=>Path.Combine(Application.persistentDataPath,"VITQRA","ResearchEvidence");
        private static string EvidenceKey(VitqraChallengeDirectory.Item item)=>item.id+"\n"+item.url;
        private string EvidenceSuffix(VitqraChallengeDirectory.Item item)
        {
            string key=EvidenceKey(item);
            string suffix=evidenceStates.TryGetValue(key,out var state)?" · "+T(state):"";
            if(reportStates.TryGetValue(key,out var saved))suffix+=" · "+T(saved?D.ReportSaved:D.ReportUnavailable);
            if(draftStates.TryGetValue(key,out var draft))suffix+=" · "+T(draft);
            return packageStates.TryGetValue(key,out var ready)?suffix+" · "+T(ready?D.PackageSaved:D.PackageUnavailable):suffix;
        }
        private void QueueManualEvidence(VitqraChallengeDirectory.Item item)
        {
            if(EligibleForProvider(item)&&!evidenceStates.ContainsKey(EvidenceKey(item)))manualEvidence=item;
        }
        private void RestoreRunControl()
        {
            int value=PlayerPrefs.GetInt(RunPreference,0);
            runPaused=value==1;runStopped=value==2;
        }
        private void SaveRunControl()
        {
            PlayerPrefs.SetInt(RunPreference,runStopped?2:runPaused?1:0);
            PlayerPrefs.Save();
        }
        private void RenderRunControls()
        {
            pauseLabel.text=T(runPaused||runStopped?D.RunResume:D.RunPause);
            SetPageButton(pauseLabel,pauseBox,true);
            SetPageButton(stopLabel,stopBox,!runStopped);
        }
        private void ToggleRunPause()
        {
            if(runPaused||runStopped){runPaused=false;runStopped=false;}
            else{runPaused=true;CancelEvidence();}
            SaveRunControl();Render();
        }
        private void StopRun()
        {
            if(runStopped)return;
            runStopped=true;runPaused=false;CancelEvidence();
            SaveRunControl();Render();
        }
        private void UpdateEvidence()
        {
            if((!localResearchSelected&&!ExternalReady)||(activePackage!=null&&!activePackage.IsCompleted)||(activeDraft!=null&&!activeDraft.IsCompleted)||runPaused||runStopped||evidencePaused||evidenceBusy||Time.unscaledTime<evidenceNext||evidenceJobs>=VitqraEvidence.MaxJobs)return;
            VitqraChallengeDirectory.Item item=null;
            if(automatic)
            {
                manualEvidence=null;
                foreach(var candidate in filtered)
                    if(EligibleForProvider(candidate)&&!evidenceStates.ContainsKey(EvidenceKey(candidate)))
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
            if(result!=null&&result.success)
            {
                var review=VitqraEvidenceReview.Inspect(EvidenceRoot,item);
                D state=review.state==VitqraEvidenceReview.State.DocumentChecksPassed?
                    D.DocumentChecksPassed:review.state==VitqraEvidenceReview.State.Partial?
                    D.DocumentReviewPartial:D.DocumentReviewFailed;
                if(item.educational&&VitqraSampleSolver.Supports(item.id)&&review.state==VitqraEvidenceReview.State.DocumentChecksPassed)
                {
                    var check=VitqraSampleSolver.Run(EvidenceRoot,ResultRoot,item);
                    if(check==VitqraSampleSolver.State.Checked&&item.id==VitqraMotifBenchmark.ChallengeId)
                        VitqraMotifBenchmark.Save(EvidenceRoot,ResultRoot,item);
                    state=check==VitqraSampleSolver.State.Checked?D.SampleChecked:
                        check==VitqraSampleSolver.State.Mismatch?D.SampleMismatch:D.SamplePending;
                }
                if(review.state==VitqraEvidenceReview.State.DocumentChecksPassed||review.state==VitqraEvidenceReview.State.Partial)
                    reportStates[EvidenceKey(item)]=VitqraResearchReport.Save(EvidenceRoot,ResultRoot,ReportRoot,item,locale);
                if((review.state==VitqraEvidenceReview.State.DocumentChecksPassed||review.state==VitqraEvidenceReview.State.Partial)&&(localResearchSelected?researchAi!=null&&researchAi.Configured:ExternalReady))
                {
                    string key=EvidenceKey(item);
                    draftStates[key]=D.DraftGenerating;Render();
                    activeDraft=GenerateDraft(item);
                    while(!activeDraft.IsCompleted)yield return null;
                    draftStates[key]=activeDraft.Status==TaskStatus.RanToCompletion&&activeDraft.Result?D.DraftSaved:D.DraftUnavailable;
                    activeDraft=null;
                    if(!localResearchSelected&&draftStates[key]!=D.DraftSaved)
                    {runPaused=true;SaveRunControl();}
                }
                if(VitqraResearchPackage.Available&&reportStates.TryGetValue(EvidenceKey(item),out var reportSaved)&&reportSaved)
                {
                    activePackage=SavePackage(item);
                    while(!activePackage.IsCompleted)yield return null;
                    packageStates[EvidenceKey(item)]=activePackage.Status==TaskStatus.RanToCompletion&&activePackage.Result;
                    activePackage=null;
                }
                evidenceStates[EvidenceKey(item)]=state;
            }
            else evidenceStates[EvidenceKey(item)]=D.EvidenceFailed;
            evidenceBusy=false;evidenceRoutine=null;evidenceNext=Time.unscaledTime+2f;Render();
        }
        private void CancelEvidence()
        {
            if(packageCancel!=null)packageCancel.Cancel();
            if(draftCancel!=null)draftCancel.Cancel();
            if(!localResearchSelected&&!ollamaResearchSelected)VitqraChatAccount.CancelResearch();
            if(evidenceRequest!=null)evidenceRequest.Abort();
            if(evidenceRoutine!=null)StopCoroutine(evidenceRoutine);
            evidenceRequest=null;evidenceRoutine=null;evidenceBusy=false;manualEvidence=null;
            var canceled=new List<string>();
            foreach(var pair in evidenceStates)if(pair.Value==D.EvidenceLoading)canceled.Add(pair.Key);
            foreach(var key in canceled){evidenceStates.Remove(key);draftStates.Remove(key);packageStates.Remove(key);}
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
