using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using D = Vitqra.VitqraDiscoveryStrings.Key;

namespace Vitqra
{
    public sealed partial class VitqraDiscoveryPanel : VitqraSpatialPanel
    {
        public override float BaseWidth => .86f;
        public override float BaseHeight => .90f;
        private const int VisibleRows=VitqraResultPages.PageSize;
        private string locale,query="",editBuffer="";
        private Text searchLabel,status,manualLabel,automaticLabel,resultCount,pageLabel,previousLabel,nextLabel;
        private Image previousBox,nextBox;
        private Image background,manualBox,automaticBox;
        private readonly Text[] areaLabels=new Text[11];
        private readonly Image[] areaBoxes=new Image[11];
        private readonly Text[] rowLabels=new Text[VisibleRows];
        private readonly Image[] rowBoxes=new Image[VisibleRows];
        private readonly List<Rect> targets=new List<Rect>();
        private readonly List<Action> actions=new List<Action>();
        private List<VitqraChallengeDirectory.Item> all=new List<VitqraChallengeDirectory.Item>();
        private List<VitqraChallengeDirectory.Item> filtered=new List<VitqraChallengeDirectory.Item>();
        private int area=-1,first;
        private bool automatic,loading,partial,unavailable,desktopEditing;
        private TouchScreenKeyboard keyboard;
        private float lastRefresh=-1000f,nextScroll;
        private Action<bool> modeChanged;
        private string webEndpoint;
        private bool webEnabled,webLoading,webFailed,webQueued;
        private int searchRevision;
        private float searchAt;
        private List<VitqraChallengeDirectory.Item> webResults=new List<VitqraChallengeDirectory.Item>();
        public void ConfigureWebSearch(string endpoint,bool allowPublicSearchTerms)
        {
            webEndpoint=endpoint;
            webEnabled=allowPublicSearchTerms&&VitqraWebDiscovery.ValidEndpoint(endpoint);
        }
        private string T(D key)=>VitqraDiscoveryStrings.Get(locale,key);
        private static readonly Color Normal=new Color(.10f,.20f,.28f,1);
        private static readonly Color Selected=new Color(.12f,.40f,.42f,1);

        public void InitializeDiscovery(string language,bool automaticResearch,Action<bool> changeMode)
        {
            locale=language;automatic=automaticResearch;modeChanged=changeMode;
            var obj=new GameObject("Discovery Canvas",typeof(RectTransform),typeof(Canvas));
            discoveryCanvas=obj;
            var rect=(RectTransform)obj.transform;
            rect.SetParent(transform,false);
            rect.sizeDelta=new Vector2(860,900);
            rect.localScale=Vector3.one*.001f;
            obj.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
            background=Box("Background",rect,Vector2.zero,new Vector2(860,900),new Color(.025f,.065f,.10f,.97f));
            Box("Accent",rect,new Vector2(0,446),new Vector2(860,8),Color.cyan);
            Label("Heading",rect,new Vector2(0,405),new Vector2(820,48),T(D.Title),31,Color.cyan);
            searchLabel=Button(rect,0,343,810,56,T(D.SearchAreas),EditQuery,out _);
            manualLabel=Button(rect,-205,277,390,50,T(D.Manual),()=>SetMode(false),out manualBox);
            automaticLabel=Button(rect,205,277,390,50,T(D.Automatic),()=>SetMode(true),out automaticBox);
            Label("Areas heading",rect,new Vector2(-266,222),new Vector2(280,45),T(D.Areas),23,Color.cyan);
            var resultsHeading=Label("Results heading",rect,new Vector2(65,222),new Vector2(350,45),T(D.ResultsHeading),23,Color.cyan);
            resultsHeading.alignment=TextAnchor.MiddleLeft;
            resultCount=Label("Result count",rect,new Vector2(350,222),new Vector2(100,45),"",20,Color.white);
            resultCount.alignment=TextAnchor.MiddleRight;
            for(int i=0;i<areaLabels.Length;i++)
            {
                int selected=i-1;
                var label=i==0?T(D.All):T((D)((int)D.Bioinformatics+i-1));
                areaLabels[i]=Button(rect,-266,174-i*49,280,45,label,()=>SelectArea(selected),out areaBoxes[i]);
            }
            for(int i=0;i<VisibleRows;i++)
            {
                int slot=i;
                rowLabels[i]=Button(rect,145,160-i*86,510,80,"",()=>OpenRow(slot),out rowBoxes[i]);
                rowLabels[i].alignment=TextAnchor.MiddleLeft;
            }
            previousLabel=Button(rect,-25,-338,170,40,T(D.ResultPrevious),()=>TurnResultPage(-1),out previousBox);
            pageLabel=Label("Results page",rect,new Vector2(145,-338),new Vector2(150,40),"",18,Color.white);
            nextLabel=Button(rect,315,-338,170,40,T(D.ResultNext),()=>TurnResultPage(1),out nextBox);
            status=Label("Status",rect,new Vector2(0,-403),new Vector2(810,72),"",19,Color.white);
            InitializeReader();
            all=VitqraChallengeDirectory.Educational();
            ApplyFilter();
        }
        private Text Button(RectTransform rect,float x,float y,float width,float height,string label,Action callback,out Image box)
        {
            box=Box("Button",rect,new Vector2(x,y),new Vector2(width,height),Normal);
            var text=Label("Button label",rect,new Vector2(x,y),new Vector2(width-16,height-6),label,20,Color.white);
            targets.Add(new Rect((x-width*.5f)*.001f,(y-height*.5f)*.001f,width*.001f,height*.001f));
            actions.Add(callback);
            return text;
        }
        public override void Highlight(bool value)
        {
            if(background!=null)background.color=value?new Color(.07f,.20f,.24f,.98f):new Color(.025f,.065f,.10f,.97f);
        }
        public override bool Intersect(Ray ray,out float distance,out int region)
        {
            distance=0;region=0;
            if(!gameObject.activeInHierarchy||Vector3.Dot(ray.direction,transform.forward)<=.0001f)return false;
            if(!new Plane(transform.forward,transform.position).Raycast(ray,out distance)||distance>4f)return false;
            var p=transform.InverseTransformPoint(ray.GetPoint(distance));
            if(Mathf.Abs(p.x)>BaseWidth*.5f||Mathf.Abs(p.y)>BaseHeight*.5f)return false;
            var hitTargets=graphicOpen?graphicTargets:readerOpen?readerTargets:targets;
            for(int i=0;i<hitTargets.Count;i++)
                if(hitTargets[i].Contains(new Vector2(p.x,p.y))){region=i+1;break;}
            return true;
        }
        public override void InvokeAction(int region)
        {
            var hitActions=graphicOpen?graphicActions:readerOpen?readerActions:actions;
            if(region>0&&region<=hitActions.Count)hitActions[region-1]?.Invoke();
        }
        public void RefreshSources()
        {
            if(loading||Time.unscaledTime-lastRefresh<30f||!gameObject.activeInHierarchy)return;
            lastRefresh=Time.unscaledTime;
            StartCoroutine(RefreshRoutine());
        }
        private IEnumerator RefreshRoutine()
        {
            loading=true;unavailable=false;partial=false;Render();
            var next=VitqraChallengeDirectory.Educational();
            int success=0;
            foreach(var term in VitqraDiscovery.Topics)
            {
                VitqraDiscovery.Batch batch=null;
                yield return VitqraDiscovery.Fetch(term,b=>batch=b);
                if(batch!=null&&batch.success)
                {
                    success++;partial|=batch.limited;
                    foreach(var n in batch.items)next.Add(new VitqraChallengeDirectory.Item {
                        id="fr-"+n.document_number,title=n.title,url=n.html_url,keywords="",educational=false,
                        officialPdf=n.pdf_url,publicationDate=n.publication_date});
                    // Publish results as each source query returns, without waiting for all queries.
                    all=next;ApplyFilter();
                }
                else partial=true;
            }
            all=next;loading=false;unavailable=success==0;ApplyFilter();
        }
        private void SelectArea(int selected)
        {
            CancelEvidence();manualEvidence=null;
            area=selected;query="";QueueWebSearch();ApplyFilter(true);
        }
        private void ApplyFilter(bool resetPage=false)
        {
            filtered=VitqraChallengeDirectory.Filter(all,area,query);
            var urls=new HashSet<string>(StringComparer.Ordinal);
            foreach(var item in filtered)urls.Add(item.url);
            foreach(var item in webResults)
                if(VitqraChallengeDirectory.ValidLink(item)&&urls.Add(item.url))filtered.Add(item);
            first=resetPage?0:VitqraResultPages.ClampFirst(first,filtered.Count);Render();
        }
        private void QueueWebSearch()
        {
            searchRevision++;
            webResults.Clear();webFailed=false;
            webQueued=webEnabled&&(area>=0||!string.IsNullOrWhiteSpace(query));
            searchAt=Time.unscaledTime+.65f;
        }
        private IEnumerator SearchWeb(int revision,string term,string language)
        {
            webLoading=true;Render();
            int success=0;
            foreach(string kind in new[]{"challenge","competition"})
            {
                if(revision!=searchRevision)break;
                VitqraWebDiscovery.Batch batch=null;
                yield return VitqraWebDiscovery.Fetch(webEndpoint,term,language,kind,b=>batch=b);
                if(revision!=searchRevision)break;
                if(batch!=null&&batch.success)
                {
                    success++;
                    webResults.AddRange(batch.items);
                    ApplyFilter();
                }
                else webFailed=true;
            }
            webLoading=false;
            if(revision==searchRevision){webFailed|=success==0;Render();}
        }
        public void SyncMode(bool value){automatic=value;Render();}
        private void SetMode(bool value){if(automatic!=value)CancelEvidence();SyncMode(value);modeChanged?.Invoke(value);}
        public void Scroll(float y)
        {
            if(Mathf.Abs(y)<.55f||Time.unscaledTime<nextScroll)return;
            nextScroll=Time.unscaledTime+.22f;
            if(graphicOpen)return;
            if(readerOpen){TurnReaderPage(y<0?1:-1);return;}
            TurnResultPage(y<0?1:-1);
        }
        private void TurnResultPage(int direction)
        {
            first=VitqraResultPages.Move(first,filtered.Count,direction);
            Render();
        }
        private static void SetPageButton(Text label,Image box,bool available)
        {
            label.color=available?Color.white:new Color(.55f,.63f,.68f,1f);
            box.color=available?Normal:new Color(.06f,.12f,.17f,1f);
        }
        private void Render()
        {
            if(status==null)return;
            first=VitqraResultPages.ClampFirst(first,filtered.Count);
            resultCount.text=filtered.Count.ToString();
            pageLabel.text=VitqraResultPages.PageNumber(first,filtered.Count)+" / "+VitqraResultPages.PageCount(filtered.Count);
            SetPageButton(previousLabel,previousBox,first>0);
            SetPageButton(nextLabel,nextBox,first+VisibleRows<filtered.Count);
            searchLabel.text=string.IsNullOrWhiteSpace(query)?T(D.SearchAreas):query;
            manualBox.color=automatic?Normal:Selected;
            automaticBox.color=automatic?Selected:Normal;
            manualLabel.text=(automatic?"":"● ")+T(D.Manual);
            automaticLabel.text=(automatic?"● ":"")+T(D.Automatic);
            for(int i=0;i<areaLabels.Length;i++)areaBoxes[i].color=area==i-1?Selected:Normal;
            for(int i=0;i<VisibleRows;i++)
            {
                int n=first+i;
                bool exists=n<filtered.Count;
                rowBoxes[i].gameObject.SetActive(exists);
                rowLabels[i].gameObject.SetActive(exists);
                if(!exists)continue;
                var item=filtered[n];
                string name=item.title.Length>110?item.title.Substring(0,110)+"…":item.title;
                rowLabels[i].supportRichText=false;
                rowLabels[i].text=(n+1)+". "+name+"\n"+T(item.webCandidate?D.WebCandidate:item.educational?D.Educational:D.Candidate)+EvidenceSuffix(item);
            }
            string summary=filtered.Count==0?T(loading?D.Searching:D.NoChallenges):
                (first+1)+"–"+Mathf.Min(first+VisibleRows,filtered.Count)+" / "+filtered.Count;
            if(unavailable)summary+=" · "+T(D.Unavailable);
            else if(partial)summary+=" · "+T(D.Limited);
            if(webLoading||webQueued)summary+=" · "+T(D.WebSearching);
            else if(webFailed)summary+=" · "+T(D.WebUnavailable);
            else if(!webEnabled)summary+=" · "+T(D.WebNotConfigured);
            status.text=summary+"\n"+T(D.EnginePending)+(evidenceBusy?" · "+T(D.EvidenceLoading):" · "+T(D.OriginalTitles));
        }
        private void OpenRow(int slot)
        {
            int n=first+slot;
            if(n>=filtered.Count)return;
            var item=filtered[n];
            if(!automatic)QueueManualEvidence(item);
            if(VitqraChallengeDirectory.ValidLink(item)&&!TryOpenGraphic(item))OpenReader(item);
        }
        private void EditQuery()
        {
            if(keyboard!=null||desktopEditing)return;
            if(TouchScreenKeyboard.isSupported)
            {
                keyboard=TouchScreenKeyboard.Open(query,TouchScreenKeyboardType.Default,false,false,false,false,T(D.SearchAreas),120);
                if(keyboard!=null)return;
            }
#if UNITY_EDITOR || UNITY_STANDALONE
            editBuffer=query;desktopEditing=true;
#else
            status.text=T(D.KeyboardUnavailable);
#endif
        }
        private void SetQuery(string value)
        {
            CancelEvidence();manualEvidence=null;
            query=(value??"").Trim();
            if(query.Length>120)query=query.Substring(0,120);
            area=-1;
            // A localized area name typed in the search field selects that area.
            for(int i=0;i<10;i++)
                if(string.Equals(query,T((D)((int)D.Bioinformatics+i)),StringComparison.OrdinalIgnoreCase))
                {area=i;query="";break;}
            QueueWebSearch();ApplyFilter(true);
        }
        private void Update()
        {
            UpdateGraphic();
            UpdateEvidence();
            if(webQueued&&!webLoading&&Time.unscaledTime>=searchAt)
            {
                webQueued=false;
                // Area queries use reviewed English terms. Typed keywords retain their language.
                string term=area>=0?VitqraChallengeDirectory.AreaTerms[area][0]:query;
                StartCoroutine(SearchWeb(searchRevision,term,area>=0?"en":VitqraStrings.ResolveLanguage(locale)));
            }
            if(keyboard==null)return;
            if(keyboard.status==TouchScreenKeyboard.Status.Done){SetQuery(keyboard.text);keyboard=null;}
            else if(keyboard.status==TouchScreenKeyboard.Status.Canceled||keyboard.status==TouchScreenKeyboard.Status.LostFocus)keyboard=null;
        }
        private void OnGUI()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if(!desktopEditing)return;
            GUILayout.BeginArea(new Rect(20,20,Mathf.Max(100,Mathf.Min(560,Screen.width-40)),150),GUI.skin.box);
            GUILayout.Label(T(D.SearchAreas));
            editBuffer=GUILayout.TextField(editBuffer,120);
            if(GUILayout.Button(T(D.Done))){SetQuery(editBuffer);desktopEditing=false;}
            if(GUILayout.Button(T(D.Cancel)))desktopEditing=false;
            GUILayout.EndArea();
#endif
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            if(keyboard!=null){keyboard.active=false;keyboard=null;}
            desktopEditing=false;
            CancelEvidence();
            CloseGraphic();
            CloseReader();
            StopAllCoroutines();loading=false;webLoading=false;webQueued=false;searchRevision++;
        }
    }
}
