using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using K=Vitqra.VitqraProviderStrings.Key;

namespace Vitqra
{
    // Same size and layout slot as the original settings panel.
    public sealed class VitqraAiSettingsPanel:VitqraSpatialPanel
    {
        private readonly List<Rect> targets=new List<Rect>();
        private readonly List<Action> actions=new List<Action>();
        private Image localBox,chatBox,chatCard,ollamaBox;
        private Text ollamaState,ollamaChoice;
        private bool ollamaSelected,ollamaPage;
        private int ollamaTargets;
        private readonly List<GameObject> ollamaObjects=new List<GameObject>();
        private Text localStatus;
        private Text status,translation,signIn,signOut,usage;
        private Image signInBox,signOutBox,usageBox,modelsBox;
        private Text modelsLabel,modelTitle,modelStatus,modelChoice,modelLimit,modelTest,modelBack,modelRefresh;
        private readonly List<GameObject> modelObjects=new List<GameObject>(), normalObjects=new List<GameObject>();
        private bool modelPage;
        private int modelIndex=-1;
        private string selectedSlug{get=>VitqraChatAccount.ManualModel;set=>VitqraChatAccount.ManualModel=value;}
        private Text researchConsentLabel;
        private int normalTargets;
        private bool chatSelected;
        private float nextPoll;
        private string locale;
        private static readonly Color Selected=new Color(.10f,.32f,.34f,1);
        private static readonly Color NormalButton=new Color(.09f,.17f,.24f,1);
        private string T(K key)=>VitqraProviderStrings.Get(locale,key);
        public void InitializeSettings(string language,Action<string> select,Action toggleTranslation,Action avatar)
        {
            locale=language;
            var obj=new GameObject("AI Settings Canvas",typeof(RectTransform),typeof(Canvas));
            var rect=(RectTransform)obj.transform;rect.SetParent(transform,false);
            rect.sizeDelta=new Vector2(640,340);rect.localScale=Vector3.one*.001f;
            obj.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
            Box("Background",rect,Vector2.zero,new Vector2(640,340),new Color(.025f,.065f,.10f,.98f));
            Box("Accent",rect,new Vector2(0,166),new Vector2(640,8),new Color(.75f,.64f,1f));
            Label("Heading",rect,new Vector2(0,135),new Vector2(600,42),T(K.Title),28,Color.cyan);
            Button(rect,-150,96,290,36,T(K.Local),()=>{VitqraChatAccount.Cancel();VitqraChatAccount.SetResearchConsent(false);select("local");},out localBox);
            localStatus=Label("Local model state",rect,new Vector2(0,8),new Vector2(570,42),"",16,Color.white);
            localStatus.supportRichText=false;
            // One shared visual container for the account heading, state and controls.
            chatCard=Box("ChatGPT account group",rect,new Vector2(0,-1),new Vector2(600,144),new Color(.08f,.22f,.26f,1));
            Button(rect,0,49,584,36,T(K.ChatGpt),()=>select("chatgpt"),out chatBox);
            status=Label("Account state",rect,new Vector2(0,8),new Vector2(570,42),"",17,Color.white);
            status.supportRichText=false;
            signIn=Button(rect,-219,-39,142,40,T(K.SignIn),()=>{if(chatSelected&&VitqraChatAccount.Status!=VitqraChatAccount.State.Busy)VitqraChatAccount.SignIn();},out signInBox);
            signOut=Button(rect,-73,-39,142,40,T(K.SignOut),()=>{if(chatSelected&&VitqraChatAccount.Status!=VitqraChatAccount.State.Busy)VitqraChatAccount.SignOut();},out signOutBox);
            usage=Button(rect,73,-39,142,40,T(K.Usage),()=>{if(chatSelected)Application.OpenURL("https://chatgpt.com/settings/usage");},out usageBox);
            modelsLabel=Button(rect,219,-39,142,40,T(K.Models),OpenModels,out modelsBox);
            translation=Button(rect,0,-96,590,36,"",toggleTranslation,out _);
            Button(rect,0,-139,590,38,T(K.Avatar),avatar,out _);
            Button(rect,150,96,290,36,"Ollama",()=>{select("ollama");OpenOllama();},out ollamaBox);
            normalTargets=targets.Count;
            foreach(Transform child in rect)if(child.name!="Background"&&child.name!="Accent"&&child.name!="Heading")normalObjects.Add(child.gameObject);
            int start=rect.childCount;
            researchConsentLabel=Button(rect,0,96,590,32,"",()=>{
                VitqraChatAccount.SetResearchConsent(!VitqraChatAccount.ResearchConsent);UpdateModels();
            },out _);
            researchConsentLabel.fontSize=15;
            modelStatus=Label("Model state",rect,new Vector2(0,63),new Vector2(590,34),"",16,Color.white);
            modelStatus.supportRichText=false;
            modelChoice=Button(rect,0,21,590,42,"",NextModel,out _);
            modelLimit=Button(rect,0,-25,590,40,"",()=>{VitqraChatAccount.CycleLimit();UpdateModels();},out _);
            modelRefresh=Button(rect,-150,-73,290,42,T(K.RefreshModels),()=>VitqraChatAccount.RefreshCatalog(),out _);
            modelTest=Button(rect,150,-73,290,42,T(K.TestConnection),TestModel,out _);
            modelBack=Button(rect,0,-130,590,42,T(K.Back),CloseModels,out _);
            for(int i=start;i<rect.childCount;i++){var child=rect.GetChild(i).gameObject;modelObjects.Add(child);child.SetActive(false);}
            ollamaTargets=targets.Count;
            start=rect.childCount;
            Label("Ollama heading",rect,new Vector2(0,96),new Vector2(590,30),"Ollama",22,Color.cyan);
            ollamaState=Label("Ollama state",rect,new Vector2(0,49),new Vector2(590,62),"",16,Color.white);
            ollamaState.supportRichText=false;
            ollamaChoice=Button(rect,0,-9,590,42,"",()=>{VitqraOllama.Cycle();UpdateOllama();},out _);
            Button(rect,0,-65,590,42,T(K.RefreshModels),()=>{_=VitqraOllama.Refresh();},out _);
            Button(rect,0,-130,590,42,T(K.Back),CloseOllama,out _);
            for(int i=start;i<rect.childCount;i++){var child=rect.GetChild(i).gameObject;ollamaObjects.Add(child);child.SetActive(false);}
        }
        private Text Button(RectTransform parent,float x,float y,float width,float height,string label,Action action,out Image box)
        {
            box=Box("Setting action",parent,new Vector2(x,y),new Vector2(width,height),NormalButton);
            var t=Label("Setting label",parent,new Vector2(x,y),new Vector2(width-10,height-4),label,20,Color.white);
            t.supportRichText=false;
            targets.Add(new Rect((x-width/2)*.001f,(y-height/2)*.001f,width*.001f,height*.001f));
            actions.Add(action);return t;
        }
        public void RefreshSettings(VitqraSettings settings,bool modelReady)
        {
            if(status==null)return;
            bool local=settings.LocalAiSelected;
            localBox.color=local?Selected:NormalButton;
            chatSelected=settings.preferredAi=="chatgpt";ollamaSelected=settings.preferredAi=="ollama";
            chatBox.color=chatSelected?Selected:NormalButton;ollamaBox.color=ollamaSelected?Selected:NormalButton;
            localStatus.text=local?T(modelReady?K.Ready:K.NotReady):ollamaSelected?OllamaSummary():"";
            status.text=T(K.Disconnected);
            UpdateAccountStatus();
            translation.text=T(K.Translate)+": "+T(settings.automaticTranslation?K.On:K.Off);
        }
        private void Update()
        {
            if(Time.unscaledTime<nextPoll)return;
            nextPoll=Time.unscaledTime+.5f;
            if(ollamaPage)UpdateOllama();
            else if(chatSelected){if(modelPage)UpdateModels();else UpdateAccountStatus();}
            else if(ollamaSelected)localStatus.text=OllamaSummary();
        }
        private string OllamaSummary()
        {
            if(VitqraOllama.Status==VitqraOllama.State.Unconfigured)return T(K.OllamaSetup);
            if(VitqraOllama.Status==VitqraOllama.State.Loading)return T(K.LoadingModels);
            if(VitqraOllama.Status==VitqraOllama.State.Unavailable)return T(K.ConnectionFailed);
            return (VitqraOllama.Selected==null?T(K.OllamaReview):VitqraOllama.Selected.name)+" · "+VitqraOllama.InstalledCount;
        }
        private void OpenOllama()
        {
            if(!ollamaSelected)return;
            ollamaPage=true;
            foreach(var obj in normalObjects)obj.SetActive(false);
            foreach(var obj in ollamaObjects)obj.SetActive(true);
            UpdateOllama();
        }
        private void CloseOllama()
        {
            ollamaPage=false;
            foreach(var obj in ollamaObjects)obj.SetActive(false);
            foreach(var obj in normalObjects)obj.SetActive(true);
            UpdateAccountStatus();localStatus.text=OllamaSummary();
        }
        private void UpdateOllama()
        {
            ollamaState.text=VitqraOllama.Server+"\n"+OllamaSummary();
            ollamaChoice.text=(VitqraOllama.Automatic?T(K.AutoModel)+": ":"")+
                (VitqraOllama.Selected==null?T(K.OllamaReview):VitqraOllama.Selected.name);
        }
        private void OpenModels()
        {
            if(!chatSelected)return;
            modelPage=true;
            foreach(var obj in normalObjects)obj.SetActive(false);
            foreach(var obj in modelObjects)obj.SetActive(true);
            if(VitqraChatAccount.CatalogState==0&&VitqraChatAccount.Status!=VitqraChatAccount.State.Busy)VitqraChatAccount.RefreshCatalog();
            UpdateModels();
        }
        private void CloseModels()
        {
            modelPage=false;
            foreach(var obj in modelObjects)obj.SetActive(false);
            foreach(var obj in normalObjects)obj.SetActive(true);
            UpdateAccountStatus();
        }
        private void NextModel()
        {
            var models=VitqraChatAccount.Catalog;
            if(models.Length==0||VitqraChatAccount.Status==VitqraChatAccount.State.Busy)return;
            modelIndex=Array.FindIndex(models,m=>m.slug==selectedSlug);
            modelIndex=(modelIndex+2)%(models.Length+1)-1;
            selectedSlug=modelIndex<0?"":models[modelIndex].slug;
            UpdateModels();
        }
        private string ResolveModel(VitqraChatAccount.Model[] models)
        {
            return VitqraResearchModelSelector.Resolve(models,selectedSlug);
        }
        private void TestModel()
        {
            string slug=ResolveModel(VitqraChatAccount.Catalog);
            if(!string.IsNullOrEmpty(slug))VitqraChatAccount.TestConnection(slug);
        }
        private void UpdateModels()
        {
            var models=VitqraChatAccount.Catalog;
            string slug=ResolveModel(models);
            var model=Array.Find(models,m=>m.slug==slug);
            bool unranked=string.IsNullOrEmpty(selectedSlug)&&models.Length>0&&string.IsNullOrEmpty(slug);
            modelChoice.text=(string.IsNullOrEmpty(selectedSlug)?T(K.AutoModel)+": ":"")+(model==null?T(unranked?K.UnrankedModels:K.NoModels):model.name);
            modelLimit.text=T(K.DailyAttempts)+": "+VitqraChatAccount.Used+" / "+VitqraChatAccount.DailyLimit+" (UTC)";
            int test=VitqraChatAccount.TestState;
            K state=test==1?K.TestRunning:test==2?K.TestPassed:test==4?K.ProviderLimit:test==5?K.LocalLimit:test==3?K.TestFailed:
                VitqraChatAccount.CatalogState==1?K.LoadingModels:VitqraChatAccount.CatalogState==3?K.NoModels:K.TestNote;
            modelStatus.text=T(state);
            researchConsentLabel.text=T(K.ResearchConsent)+": "+T(VitqraChatAccount.ResearchConsent?K.On:K.Off);
        }
        private void UpdateAccountStatus()
        {
            if(signIn==null)return;
            if(modelPage||ollamaPage)return;
            localStatus.gameObject.SetActive(!chatSelected);
            chatCard.gameObject.SetActive(chatSelected);
            status.gameObject.SetActive(chatSelected);
            foreach(var t in new[]{signIn,signOut,usage,modelsLabel})t.gameObject.SetActive(chatSelected);
            foreach(var b in new[]{signInBox,signOutBox,usageBox,modelsBox})b.gameObject.SetActive(chatSelected);
            if(!chatSelected)return;
            var state=VitqraChatAccount.Status;
            K key=K.Disconnected;
            switch(state)
            {
                case VitqraChatAccount.State.Busy:key=K.Connecting;break;
                case VitqraChatAccount.State.PlanGranted:key=K.PlanGranted;break;
                case VitqraChatAccount.State.IdentityOnly:key=K.IdentityOnly;break;
                case VitqraChatAccount.State.Failed:key=K.ConnectionFailed;break;
                case VitqraChatAccount.State.Expired:key=K.Expired;break;
                case VitqraChatAccount.State.Unsupported:key=K.AndroidOnly;break;
                case VitqraChatAccount.State.RevocationUnconfirmed:key=K.RevocationUnconfirmed;break;
            }
            status.text=T(key);
            if(state==VitqraChatAccount.State.PlanGranted&&VitqraChatAccount.CatalogState==2)
            {
                var models=VitqraChatAccount.Catalog;
                string slug=ResolveModel(models);
                var selected=Array.Find(models,model=>model.slug==slug);
                if(selected!=null)status.text+="\n"+selected.name;
            }
            bool ready=state!=VitqraChatAccount.State.Busy&&state!=VitqraChatAccount.State.Unsupported;
            signIn.color=signOut.color=ready?Color.white:Color.gray;
        }
        public override bool Intersect(Ray ray,out float distance,out int region)
        {
            distance=0;region=0;
            if(!gameObject.activeInHierarchy||Vector3.Dot(ray.direction,transform.forward)<=.0001f||
               !new Plane(transform.forward,transform.position).Raycast(ray,out distance)||distance>4f)return false;
            Vector3 local=transform.InverseTransformPoint(ray.GetPoint(distance));
            if(Mathf.Abs(local.x)>Width*.5f||Mathf.Abs(local.y)>Height*.5f)return false;
            for(int i=0;i<targets.Count;i++)
            {
                // Account actions are not hit targets while their group is collapsed.
                if(ollamaPage){if(i<ollamaTargets)continue;}
                else if(modelPage){if(i<normalTargets||i>=ollamaTargets)continue;}
                else if(i>=normalTargets)continue;
                if(!chatSelected&&i>=2&&i<=5)continue;
                if(targets[i].Contains(new Vector2(local.x,local.y))){region=i+1;break;}
            }
            return true;
        }
        public override void InvokeAction(int region)
        {
            if(ollamaPage){if(region<=ollamaTargets)return;}
            else if(modelPage){if(region<=normalTargets||region>ollamaTargets)return;}
            else if(region>normalTargets)return;
            if(!chatSelected&&region>=3&&region<=6)return;
            if(region>0&&region<=actions.Count)actions[region-1]?.Invoke();
        }
    }
}
