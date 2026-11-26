using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using D = Vitqra.VitqraDiscoveryStrings.Key;

namespace Vitqra
{
    public sealed partial class VitqraDiscoveryPanel
    {
        private GameObject discoveryCanvas,readerCanvas;
        private readonly List<Rect> readerTargets=new List<Rect>();
        private readonly List<Action> readerActions=new List<Action>();
        private Text readerTitle,readerAddress,readerBody,readerStatus;
        private VitqraChallengeDirectory.Item readerItem;
        private List<string> readerPages=new List<string>();
        private int readerPage,readerRevision;
        private bool readerOpen,readerTruncated;
        private UnityWebRequest readerRequest;
        private Coroutine readerRoutine;

        private void InitializeReader()
        {
            readerCanvas=new GameObject("Web Reader Canvas",typeof(RectTransform),typeof(Canvas));
            var rect=(RectTransform)readerCanvas.transform;
            rect.SetParent(transform,false);
            rect.sizeDelta=new Vector2(860,900);
            rect.localScale=Vector3.one*.001f;
            readerCanvas.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
            Box("Reader Background",rect,Vector2.zero,new Vector2(860,900),new Color(.025f,.065f,.10f,.98f));
            Box("Reader Accent",rect,new Vector2(0,446),new Vector2(860,8),Color.cyan);
            Label("Reader heading",rect,new Vector2(0,413),new Vector2(820,44),T(D.Reader),30,Color.cyan);
            ReaderButton(rect,-205,353,390,52,T(D.Back),CloseReader);
            ReaderButton(rect,205,353,390,52,T(D.ExternalBrowser),OpenExternal);
            readerTitle=Label("Original title",rect,new Vector2(0,282),new Vector2(810,76),"",26,Color.white);
            readerAddress=Label("Original address",rect,new Vector2(0,221),new Vector2(810,42),"",18,Color.cyan);
            readerBody=Label("Page text",rect,new Vector2(0,-37),new Vector2(810,460),"",25,Color.white);
            readerBody.alignment=TextAnchor.UpperLeft;
            readerStatus=Label("Reader status",rect,new Vector2(0,-307),new Vector2(810,65),"",19,Color.white);
            ReaderButton(rect,-205,-389,390,54,T(D.PreviousPage),()=>TurnReaderPage(-1));
            ReaderButton(rect,205,-389,390,54,T(D.NextPage),()=>TurnReaderPage(1));
            readerCanvas.SetActive(false);
        }

        private void ReaderButton(RectTransform rect,float x,float y,float width,float height,string label,Action action)
        {
            Box("Reader button",rect,new Vector2(x,y),new Vector2(width,height),Normal);
            Label("Reader button label",rect,new Vector2(x,y),new Vector2(width-12,height-6),label,22,Color.white);
            readerTargets.Add(new Rect((x-width*.5f)*.001f,(y-height*.5f)*.001f,width*.001f,height*.001f));
            readerActions.Add(action);
        }

        private void OpenReader(VitqraChallengeDirectory.Item item)
        {
            CloseReader();
            if(!VitqraChallengeDirectory.ValidLink(item))return;
            readerItem=item;readerOpen=true;
            discoveryCanvas.SetActive(false);readerCanvas.SetActive(true);
            readerTitle.text=item.title;
            readerAddress.text=item.url;
            readerBody.text="";
            readerStatus.text=T(D.ReaderLoading);
            if(!VitqraWebReader.CanRead(item.url))
            {
                readerBody.text=T(D.ReaderUnsupported);
                readerStatus.text=T(D.UnverifiedPreview);
                return;
            }
            int revision=++readerRevision;
            readerRoutine=StartCoroutine(ReadPage(item.url,revision));
        }

        private IEnumerator ReadPage(string url,int revision)
        {
            VitqraWebReader.Result result=null;
            yield return VitqraWebReader.Fetch(url,request=>readerRequest=request,value=>result=value);
            readerRoutine=null;
            if(!readerOpen||revision!=readerRevision)yield break;
            if(result==null||!result.success)
            {
                readerBody.text=T(D.ReaderUnavailable);
                readerStatus.text=T(D.UnverifiedPreview);
                yield break;
            }
            readerPages=VitqraWebReader.Pages(result.text);
            readerTruncated=result.truncated;
            readerPage=0;RenderReader();
        }

        private void RenderReader()
        {
            if(!readerOpen||readerPages.Count==0)return;
            readerBody.text=readerPages[readerPage];
            readerStatus.text=(readerPage+1)+" / "+readerPages.Count+" · "+T(D.UnverifiedPreview)+
                (readerTruncated?" · "+T(D.TextTruncated):"");
        }

        private void TurnReaderPage(int offset)
        {
            if(readerPages.Count==0)return;
            readerPage=Mathf.Clamp(readerPage+offset,0,readerPages.Count-1);RenderReader();
        }

        private void OpenExternal()
        {
            if(readerOpen&&VitqraChallengeDirectory.ValidLink(readerItem))
                Application.OpenURL(readerItem.url);
        }

        private void CloseReader()
        {
            readerRevision++;
            if(readerRequest!=null)readerRequest.Abort();
            if(readerRoutine!=null){StopCoroutine(readerRoutine);readerRoutine=null;}
            readerRequest=null;readerOpen=false;readerItem=null;
            readerPages.Clear();readerPage=0;readerTruncated=false;
            if(readerTitle!=null)readerTitle.text="";
            if(readerAddress!=null)readerAddress.text="";
            if(readerBody!=null)readerBody.text="";
            if(readerStatus!=null)readerStatus.text="";
            if(readerCanvas!=null)readerCanvas.SetActive(false);
            if(discoveryCanvas!=null)discoveryCanvas.SetActive(true);
        }
    }
}
