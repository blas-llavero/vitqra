using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Vitqra
{
    // Explicit allowlist. No automatic startup, project changes or live tests.
    public static class VitqraIntegrationRunner
    {
        private sealed class Suite
        {
            public readonly string name;
            public readonly Action run;
            public Suite(string n,Action a){name=n;run=a;}
        }
        private static readonly Suite[] Suites={
            new Suite("AggregateCollaboration",VitqraAggregateCollaborationChecks.Run),
            new Suite("AvatarMeeting",VitqraAvatarMeetingChecks.Run),
            new Suite("Browser",VitqraBrowserChecks.Run),
            new Suite("Community",VitqraCommunityChecks.Run),
            new Suite("DailyDiary",VitqraDailyDiaryChecks.Run),
            new Suite("DataQuality",VitqraDataQualityChecks.Run),
            new Suite("Directory",VitqraDirectoryChecks.Run),
            new Suite("Discovery",VitqraDiscoveryChecks.Run),
            new Suite("EphemeralTrust",VitqraEphemeralTrustChecks.Run),
            new Suite("Evidence",VitqraEvidenceChecks.Run),
            new Suite("EvidenceReview",VitqraEvidenceReviewChecks.Run),
            new Suite("HumanImpact",VitqraHumanImpactChecks.Run),
            new Suite("Layout",VitqraLayoutChecks.Run),
            new Suite("LocalAi",VitqraLocalAiChecks.Run),
            new Suite("Longitudinal",VitqraLongitudinalChecks.Run),
            new Suite("MotifBenchmark",VitqraMotifBenchmarkChecks.Run),
            new Suite("MotifOptimization",VitqraMotifOptimizationChecks.Run),
            new Suite("Multimodal",VitqraMultimodalChecks.Run),
            new Suite("ObjectRecognition",VitqraObjectRecognitionChecks.Run),
            new Suite("Ollama",VitqraOllamaChecks.Run),
            new Suite("Previsit",VitqraPrevisitChecks.Run),
            new Suite("Provider",VitqraProviderChecks.Run),
            new Suite("Reproducibility",VitqraReproducibilityChecks.Run),
            new Suite("ResearchDraft",VitqraResearchDraftChecks.Run),
            new Suite("ResearchModelSelector",VitqraResearchModelSelectorChecks.Run),
            new Suite("ResearchReport",VitqraResearchReportChecks.Run),
            new Suite("Resize",VitqraResizeChecks.Run),
            new Suite("ResultPage",VitqraResultPageChecks.Run),
            new Suite("ReviewDossier",VitqraReviewDossierChecks.Run),
            new Suite("SampleSolver",VitqraSampleSolverChecks.Run),
            new Suite("Telepresence",VitqraTelepresenceChecks.Run),
            new Suite("ValidationCenter",VitqraValidationCenterChecks.Run),
            new Suite("WebDiscovery",VitqraWebDiscoveryChecks.Run),
            new Suite("WebReader",VitqraWebReaderChecks.Run),
            new Suite("EvidenceChain",VitqraEvidenceChainChecks.Run),
        };
        private static bool EmptyScene()
        {
            if(SceneManager.sceneCount!=1)return false;
            var scene=SceneManager.GetActiveScene();
            return scene.IsValid()&&scene.isLoaded&&scene.rootCount==0;
        }
        private static bool Ready()=>!EditorApplication.isPlayingOrWillChangePlaymode&&
            !EditorApplication.isCompiling&&!EditorApplication.isUpdating&&EmptyScene()&&
            !SceneManager.GetActiveScene().isDirty;
        [MenuItem("VITQRA/Run Integration Checks (Empty Test Scene)")]
        public static void Run()
        {
            if(!Ready()){Debug.LogWarning("Use a saved empty scene in a disposable test project, outside Play mode.");return;}
            Execute();
        }
        public static void RunBatch()
        {
            if(!Application.isBatchMode){Debug.LogWarning("Use the editor menu.");return;}
            if(!Ready()){EditorApplication.Exit(2);return;}
            EditorApplication.Exit(Execute()?0:1);
        }
        private static bool Execute()
        {
            int passed=0,failed=0;
            bool sceneSafe=true;
            foreach(var suite in Suites)
            {
                int errors=0;
                Application.LogCallback capture=(condition,stack,type)=>
                {if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors++;};
                Exception fault=null;
                Application.logMessageReceived+=capture;
                try{suite.run();}
                catch(Exception e){fault=e;}
                finally{Application.logMessageReceived-=capture;}
                if(fault==null&&errors==0){passed++;Debug.Log("VITQRA integration PASS "+suite.name);}
                else{failed++;Debug.LogError("VITQRA integration FAIL "+suite.name+" ("+(fault==null?"logged error":fault.GetType().Name)+")");}
                if(!EmptyScene()){sceneSafe=false;Debug.LogError("VITQRA integration stopped: test scene no longer empty.");break;}
            }
            int skipped=Suites.Length-passed-failed;
            Debug.Log("VITQRA integration summary: total="+Suites.Length+" passed="+passed+" failed="+failed+
                " not_run="+skipped+" scope=editor_fixtures_only; clinical_release=blocked");
            return sceneSafe&&failed==0&&skipped==0&&passed==Suites.Length;
        }
    }
}
