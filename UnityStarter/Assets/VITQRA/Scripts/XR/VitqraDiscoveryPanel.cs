using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using D = Vitqra.VitqraDiscoveryStrings.Key;

namespace Vitqra
{
    public sealed class VitqraDiscoveryPanel : VitqraSpatialPanel
    {
        private string locale,query="",editBuffer="";
        private Text title,body,footer;
        private Image background;
        private readonly List<Rect> targets=new List<Rect>();
        private readonly List<Action> actions=new List<Action>();
        private List<VitqraDiscovery.Notice> all=new List<VitqraDiscovery.Notice>(),filtered=new List<VitqraDiscovery.Notice>();
        private int index,portal;
        private bool loading,unavailable,partial,portalMode,desktopEditing;
        private bool refreshed;
        private TouchScreenKeyboard keyboard;
        private float lastRefresh=-1000f;
        private string T(D key)=>VitqraDiscoveryStrings.Get(locale,key);

        public void InitializeDiscovery(string language)
        {
            locale=language;
            var obj=new GameObject("Discovery Canvas",typeof(RectTransform),typeof(Canvas));
            var rect=(RectTransform)obj.transform;
            rect.SetParent(transform,false);
            rect.sizeDelta=new Vector2(640,340);
            rect.localScale=Vector3.one*.001f;
            obj.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
            background=Box("Background",rect,Vector2.zero,new Vector2(640,340),new Color(.025f,.065f,.10f,.97f));
            Box("Accent",rect,new Vector2(0,166),new Vector2(640,8),Color.cyan);
            title=Label("Heading",rect,new Vector2(0,135),new Vector2(610,44),T(D.Title),28,Color.cyan);
            body=Label("Result",rect,new Vector2(0,43),new Vector2(600,124),"",23,Color.white);
            footer=Label("Status",rect,new Vector2(0,-36),new Vector2(600,36),"",17,Color.white);
            Button(rect,-200,-80,T(D.Keywords),EditQuery);
            Button(rect,0,-80,T(D.Refresh),RefreshSources);
            Button(rect,200,-80,T(D.Next),Next);
            Button(rect,-200,-132,T(D.Open),OpenCurrent);
            Button(rect,0,-132,T(D.Portals),TogglePortals);
            Button(rect,200,-132,T(D.All),()=>{query="";portalMode=false;ApplyFilter();});
            Render();
        }
        private void Button(RectTransform rect,float x,float y,string label,Action callback)
        {
            Box("Button",rect,new Vector2(x,y),new Vector2(190,48),new Color(.10f,.27f,.31f,1));
            Label("Button label",rect,new Vector2(x,y),new Vector2(184,44),label,19,Color.white);
            targets.Add(new Rect((x-95)*.001f,(y-24)*.001f,.19f,.048f));
            actions.Add(callback);
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
            if(Mathf.Abs(p.x)>Width*.5f||Mathf.Abs(p.y)>Height*.5f)return false;
            for(int i=0;i<targets.Count;i++)
                if(targets[i].Contains(new Vector2(p.x,p.y))){region=i+1;break;}
            return true;
        }
        public override void InvokeAction(int region)
        {
            if(region>0&&region<=actions.Count)actions[region-1]?.Invoke();
        }
        public void RefreshSources()
        {
            if(loading||Time.unscaledTime-lastRefresh<30f||!gameObject.activeInHierarchy)return;
            lastRefresh=Time.unscaledTime;
            StartCoroutine(RefreshRoutine());
        }
        private IEnumerator RefreshRoutine()
        {
            loading=true;portalMode=false;unavailable=false;partial=false;
            all.Clear();filtered.Clear();index=0;Render();
            int success=0;
            foreach(var term in VitqraDiscovery.Topics)
            {
                VitqraDiscovery.Batch batch=null;
                yield return VitqraDiscovery.Fetch(term,b=>batch=b);
                if(batch!=null&&batch.success)
                {
                    success++;all.AddRange(batch.items);partial|=batch.limited;
                }
                else partial=true;
            }
            loading=false;refreshed=true;unavailable=success==0;
            ApplyFilter();
        }
        private void ApplyFilter()
        {
            filtered=VitqraDiscovery.Filter(all,query);index=0;Render();
        }
        private void Render()
        {
            if(title==null)return;
            title.text=T(portalMode?D.Portals:D.Title);
            if(portalMode)
            {
                body.text=T((D)((int)D.Nih+portal));
                footer.text=(portal+1)+" / "+VitqraDiscovery.PortalUrls.Length;
            }
            else if(loading){body.text=T(D.Searching);footer.text="";}
            else if(unavailable){body.text=T(D.Unavailable);footer.text="";}
            else if(filtered.Count==0)
            {
                body.text=T(refreshed?D.Empty:D.Notices);
                footer.text=partial?T(D.Limited):"";
            }
            else
            {
                var n=filtered[index];
                body.text=n.title.Length>180?n.title.Substring(0,180)+"…":n.title;
                footer.text=(index+1)+" / "+filtered.Count+" · "+n.publication_date+"\n"+T(D.Unverified);
            }
        }
        private void Next()
        {
            if(portalMode)portal=(portal+1)%VitqraDiscovery.PortalUrls.Length;
            else if(filtered.Count>0)index=(index+1)%filtered.Count;
            Render();
        }
        private void TogglePortals(){portalMode=!portalMode;Render();}
        private void OpenCurrent()
        {
            if(portalMode){Application.OpenURL(VitqraDiscovery.PortalUrls[portal]);return;}
            if(filtered.Count>0&&VitqraDiscovery.ValidSource(filtered[index].html_url))
                Application.OpenURL(filtered[index].html_url);
        }
        private void EditQuery()
        {
            if(keyboard!=null||desktopEditing)return;
            if(TouchScreenKeyboard.isSupported)
            {
                keyboard=TouchScreenKeyboard.Open(query,TouchScreenKeyboardType.Default,false,false,false,false,T(D.LocalSearch),120);
                if(keyboard!=null)return;
            }
#if UNITY_EDITOR || UNITY_STANDALONE
            editBuffer=query;desktopEditing=true;
#else
            footer.text=T(D.KeyboardUnavailable);
#endif
        }
        private void SetQuery(string value)
        {
            query=(value??"").Trim();
            if(query.Length>120)query=query.Substring(0,120);
            portalMode=false;ApplyFilter();
        }
        private void Update()
        {
            if(keyboard==null)return;
            if(keyboard.status==TouchScreenKeyboard.Status.Done){SetQuery(keyboard.text);keyboard=null;}
            else if(keyboard.status==TouchScreenKeyboard.Status.Canceled||keyboard.status==TouchScreenKeyboard.Status.LostFocus)keyboard=null;
        }
        private void OnGUI()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if(!desktopEditing)return;
            GUILayout.BeginArea(new Rect(20,20,Mathf.Max(100,Mathf.Min(560,Screen.width-40)),150),GUI.skin.box);
            GUILayout.Label(T(D.LocalSearch));
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
            StopAllCoroutines();loading=false;
        }
    }
}
