using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using D=Vitqra.VitqraDiscoveryStrings.Key;
#if VITQRA_OPENWEBVIEW && UNITY_ANDROID && !UNITY_EDITOR
using OpenWebViewKit;
#endif

namespace Vitqra
{
    public sealed partial class VitqraDiscoveryPanel
    {
        private bool graphicOpen;
        private readonly List<Rect> graphicTargets=new List<Rect>();
        private readonly List<Action> graphicActions=new List<Action>();

        // Normalized UVs use the same bottom-left convention as the upstream pointer API.
        public static bool GraphicUv(Vector2 local,out Vector2 uv)
        {
            uv=new Vector2((local.x+.405f)/.81f,(local.y+.385f)/.64f);
            return uv.x>=0&&uv.x<=1&&uv.y>=0&&uv.y<=1;
        }

#if VITQRA_OPENWEBVIEW && UNITY_ANDROID && !UNITY_EDITOR
        private GameObject graphicCanvas;
        private OpenWebView graphicWeb;
        private RawImage graphicTexture;
        private Text graphicAddress,graphicStatus;
        private VitqraChallengeDirectory.Item graphicItem;
        private TouchScreenKeyboard graphicKeyboard;
        private bool addressEditing,graphicLoaded,graphicStarted,graphicPressed;
        private float graphicStart,scrollAt;
        private Vector2 lastUv;

        private bool TryOpenGraphic(VitqraChallengeDirectory.Item item)
        {
            if(!OpenWebView.VitqraRestrictedBuild)return false;
            CloseGraphic();CloseReader();
            graphicItem=item;
            graphicOpen=true;
            graphicCanvas=new GameObject("Graphical Browser",typeof(RectTransform),typeof(Canvas));
            var rect=(RectTransform)graphicCanvas.transform;
            rect.SetParent(transform,false);
            rect.sizeDelta=new Vector2(860,900);
            rect.localScale=Vector3.one*.001f;
            graphicCanvas.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
            Box("Browser background",rect,Vector2.zero,new Vector2(860,900),new Color(.025f,.065f,.10f,.98f));
            Label("Browser heading",rect,new Vector2(0,415),new Vector2(810,42),T(D.GraphicBrowser),29,Color.cyan);
            GraphicButton(rect,-270,361,260,48,T(D.Back),CloseGraphic);
            GraphicButton(rect,0,361,260,48,T(D.Reader),UseTextReader);
            GraphicButton(rect,270,361,260,48,T(D.ExternalBrowser),()=>{if(graphicItem!=null&&VitqraChallengeDirectory.ValidLink(graphicItem))Application.OpenURL(graphicItem.url);});
            GraphicButton(rect,-340,306,128,45,"←",()=>{if(Ready()&&graphicWeb.CanGoBack)graphicWeb.Back();});
            GraphicButton(rect,-202,306,128,45,"→",()=>{if(Ready()&&graphicWeb.CanGoForward)graphicWeb.Forward();});
            GraphicButton(rect,-64,306,128,45,"↻",()=>{if(Ready())graphicWeb.Reload();});
            GraphicButton(rect,76,306,128,45,T(D.TypeText),()=>OpenGraphicKeyboard(false));
            GraphicButton(rect,214,306,128,45,T(D.EraseCharacter),()=>{if(Ready())graphicWeb.SendBackspace();});
            GraphicButton(rect,350,306,108,45,T(D.EnterKey),()=>{if(Ready())graphicWeb.SendEnter();});
            graphicAddress=GraphicButton(rect,0,263,810,34,item.url,()=>OpenGraphicKeyboard(true));
            graphicAddress.supportRichText=false;
            graphicAddress.fontSize=16;
            var image=Child("Live webpage",rect,new Vector2(0,-65),new Vector2(810,640));
            graphicTexture=image.gameObject.AddComponent<RawImage>();
            graphicTexture.uvRect=new Rect(0,1,1,-1);
            graphicTexture.color=Color.white;
            graphicStatus=Label("Browser status",rect,new Vector2(0,-414),new Vector2(810,42),T(D.ReaderLoading),18,Color.white);
            graphicStatus.supportRichText=false;
            graphicWeb=graphicCanvas.AddComponent<OpenWebView>();
            var instance=graphicWeb;
            instance.OnPageStarted+=url=>{if(graphicWeb==instance){graphicAddress.text=url;graphicStatus.text=T(D.ReaderLoading);}};
            instance.OnUrlChanged+=url=>{if(graphicWeb==instance)graphicAddress.text=url;};
            instance.OnPageLoaded+=()=>{if(graphicWeb==instance){graphicLoaded=true;graphicStatus.text=T(D.BrowserUnverified);}};
            instance.OnLoadError+=error=>{if(graphicWeb==instance&&error.IsMainFrame){graphicLoaded=true;graphicStatus.text=T(D.ReaderUnavailable);}};
            instance.OnError+=error=>{if(graphicWeb==instance)graphicStatus.text=T(D.ReaderUnavailable);};
            instance.OnPermissionRequested+=request=>request.Cancel();
            instance.OnSslError+=request=>request.Cancel();
            instance.OnHttpAuthRequest+=request=>request.Cancel();
            instance.OnFileChooser+=request=>request.Cancel();
            instance.OnJsDialog+=request=>request.Cancel();
            instance.OnDownloadRequested+=request=>{if(graphicWeb==instance)graphicStatus.text=T(D.DownloadPending);};
            graphicStart=Time.unscaledTime;graphicLoaded=false;graphicStarted=false;
            discoveryCanvas.SetActive(false);
            return true;
        }

        private Text GraphicButton(RectTransform rect,float x,float y,float width,float height,string label,Action action)
        {
            Box("Browser button",rect,new Vector2(x,y),new Vector2(width,height),Normal);
            var t=Label("Browser button label",rect,new Vector2(x,y),new Vector2(width-8,height-4),label,19,Color.white);
            t.supportRichText=false;
            graphicTargets.Add(new Rect((x-width/2)*.001f,(y-height/2)*.001f,width*.001f,height*.001f));
            graphicActions.Add(action);
            return t;
        }

        private bool Ready()=>graphicOpen&&graphicWeb!=null&&graphicWeb.IsReady&&graphicStarted;
        private void UpdateGraphic()
        {
            if(!graphicOpen||graphicWeb==null)return;
            if(!graphicStarted&&graphicWeb.IsReady)
            {
                graphicStarted=true;
                graphicWeb.SetMediaAutoplay(false);
                graphicWeb.LoadUrl(graphicItem.url);
            }
            if(graphicWeb.Texture!=null)graphicTexture.texture=graphicWeb.Texture;
            if(!graphicLoaded&&Time.unscaledTime-graphicStart>20f)
            {
                graphicLoaded=true;
                if(graphicWeb.IsReady)graphicWeb.StopLoading();
                graphicStatus.text=T(D.ReaderUnavailable);
            }
            if(graphicKeyboard==null)return;
            if(graphicKeyboard.status==TouchScreenKeyboard.Status.Done)
            {
                string value=graphicKeyboard.text;graphicKeyboard=null;
                if(!Ready())return;
                if(addressEditing)
                {
                    if(!VitqraBrowserAddress.Valid(value)){graphicStatus.text=T(D.InvalidAddress);return;}
                    graphicWeb.LoadUrl(value.Trim());
                }
                else graphicWeb.SendText(value);
            }
            else if(graphicKeyboard.status==TouchScreenKeyboard.Status.Canceled||
                graphicKeyboard.status==TouchScreenKeyboard.Status.LostFocus)graphicKeyboard=null;
        }

        private void OpenGraphicKeyboard(bool address)
        {
            if(!Ready()||graphicKeyboard!=null)return;
            if(!TouchScreenKeyboard.isSupported){graphicStatus.text=T(D.KeyboardUnavailable);return;}
            CancelGraphicPointer();addressEditing=address;
            graphicKeyboard=TouchScreenKeyboard.Open(address?graphicWeb.Url:"",TouchScreenKeyboardType.Default,
                false,false,false,false,address?T(D.Address):T(D.TypeText),2048);
            if(graphicKeyboard==null)graphicStatus.text=T(D.KeyboardUnavailable);
        }

        public void ProcessGraphicPointer(Ray ray,bool selected,bool trigger,bool previousTrigger,float scroll)
        {
            if(!Ready()||!selected||graphicKeyboard!=null||
                !new Plane(transform.forward,transform.position).Raycast(ray,out float distance)||
                !GraphicUv((Vector2)transform.InverseTransformPoint(ray.GetPoint(distance)),out var uv))
            {CancelGraphicPointer();return;}
            if(trigger&&!previousTrigger)
            {graphicWeb.PointerDown(uv);graphicPressed=true;lastUv=uv;}
            else if(trigger&&graphicPressed)
            {if((uv-lastUv).sqrMagnitude>.0001f){graphicWeb.PointerMove(uv);lastUv=uv;}}
            else if(!trigger&&graphicPressed)
            {graphicWeb.PointerUp(uv);graphicPressed=false;}
            else if(!trigger)
            {
                if((uv-lastUv).sqrMagnitude>.0001f){graphicWeb.Hover(uv);lastUv=uv;}
                if(Mathf.Abs(scroll)>.25f&&Time.unscaledTime>=scrollAt)
                {scrollAt=Time.unscaledTime+.08f;graphicWeb.Scroll(new Vector2(0,-scroll*100f),uv);}
            }
        }
        public void CancelGraphicPointer()
        {
            if(graphicPressed&&graphicWeb!=null)graphicWeb.CancelPointer(lastUv);
            graphicPressed=false;
        }
        private void UseTextReader()
        {
            var item=graphicItem;CloseGraphic();if(item!=null)OpenReader(item);
        }
        private void CloseGraphic()
        {
            CancelGraphicPointer();
            if(graphicKeyboard!=null){graphicKeyboard.active=false;graphicKeyboard=null;}
            if(graphicWeb!=null&&graphicWeb.IsReady){graphicWeb.StopLoading();graphicWeb.ClearBrowsingData();}
            if(graphicCanvas!=null){graphicCanvas.SetActive(false);Destroy(graphicCanvas);}
            graphicCanvas=null;graphicWeb=null;graphicItem=null;graphicOpen=false;
            graphicTargets.Clear();graphicActions.Clear();
            if(discoveryCanvas!=null)discoveryCanvas.SetActive(true);
        }
#else
        private bool TryOpenGraphic(VitqraChallengeDirectory.Item item)=>false;
        private void UpdateGraphic(){}
        private void CloseGraphic(){}
        public void CancelGraphicPointer(){}
        public void ProcessGraphicPointer(Ray ray,bool selected,bool trigger,bool previousTrigger,float scroll){}
#endif
    }
}
