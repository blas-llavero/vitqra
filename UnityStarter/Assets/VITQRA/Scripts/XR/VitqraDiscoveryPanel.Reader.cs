using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        private Text readerHeading,readerTitle,readerAddress,readerBody,readerStatus;
        private bool reportView,pdfView,pdfBusy;
        private RawImage pdfImage;
        private Texture2D pdfTexture;
        private CancellationTokenSource pdfCancel;
        private int pdfIndex,pdfCount;
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
            readerHeading=Label("Reader heading",rect,new Vector2(0,413),new Vector2(820,44),T(D.Reader),30,Color.cyan);
            ReaderButton(rect,-306,353,194,52,T(D.Back),CloseReader);
            ReaderButton(rect,-102,353,194,52,T(D.ViewReport),ShowReaderReport);
            ReaderButton(rect,102,353,194,52,T(D.ViewPdf),ShowPdf);
            ReaderButton(rect,306,353,194,52,T(D.ExternalBrowser),OpenExternal);
            readerTitle=Label("Original title",rect,new Vector2(0,282),new Vector2(810,76),"",26,Color.white);
            readerAddress=Label("Original address",rect,new Vector2(0,221),new Vector2(810,42),"",18,Color.cyan);
            readerBody=Label("Page text",rect,new Vector2(0,-37),new Vector2(810,460),"",25,Color.white);
            readerBody.alignment=TextAnchor.UpperLeft;
            pdfImage=Child("PDF page",rect,new Vector2(0,-35),new Vector2(352,498)).gameObject.AddComponent<RawImage>();
            pdfImage.gameObject.SetActive(false);
            readerTitle.supportRichText=false;
            readerAddress.supportRichText=false;
            readerBody.supportRichText=false;
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
            readerHeading.text=T(D.Reader);
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
            if(!readerOpen||revision!=readerRevision||pdfView||pdfBusy)yield break;
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

        private string LoadLocalReport(VitqraChallengeDirectory.Item item)
        {
            string report=VitqraResearchReport.Load(EvidenceRoot,ResultRoot,ReportRoot,item,locale);
            if(report==null)return null;
            string audit=VitqraReproducibility.Build(EvidenceRoot,ResultRoot,DraftRoot,item,locale);
            if(audit==null)return null;
            report+="\n\n"+VitqraReproducibility.Heading(locale)+"\n"+VitqraReproducibility.Scope(locale)+"\n"+audit;
            string benchmark=VitqraMotifBenchmark.Load(EvidenceRoot,ResultRoot,item);
            if(benchmark!=null)report+="\n\n"+T(D.BenchmarkHeading)+"\n"+T(D.BenchmarkScope)+"\n"+benchmark;
            var draft=VitqraResearchDraft.Load(EvidenceRoot,DraftRoot,item,locale);
            report+="\n\n"+VitqraHumanImpact.Heading(locale)+"\n"+VitqraHumanImpact.Describe(draft,locale);
            if(draft==null)return report;
            string optimization=benchmark==null?null:VitqraMotifOptimization.Describe(draft);
            if(optimization!=null)report+="\n\n"+T(D.OptimizationHeading)+"\n"+T(D.OptimizationScope)+"\n"+optimization;
            // Keep the reviewed report distinct from unvalidated AI-generated text.
            return report+"\n\n"+T(D.DraftWarning)+"\n"+T(D.DraftAlternatives)+"\n"+draft.alternatives+
                "\n\n"+T(D.DraftImpact)+"\n"+draft.critique+"\n\n"+T(D.DraftWarning)+
                "\n"+draft.ModelIdentity+"\nSHA-256 (source): "+draft.sourceSha256;
        }
        private void ShowReaderReport()
        {
            if(!readerOpen||readerItem==null)return;
            var item=readerItem;
            string report=LoadLocalReport(item);
            if(report==null){readerStatus.text=T(D.ReportUnavailable);return;}
            OpenLocalReport(item,report);
        }
        private void OpenLocalReport(VitqraChallengeDirectory.Item item,string report)
        {
            CloseReader();
            readerItem=item;readerOpen=true;reportView=true;
            readerHeading.text=T(D.ViewReport);
            readerTitle.text=item.title;
            readerAddress.text=item.url;
            readerPages=VitqraWebReader.Pages(report);
            readerPage=0;
            discoveryCanvas.SetActive(false);readerCanvas.SetActive(true);
            RenderReader();
        }
        private void RenderReader()
        {
            if(!readerOpen||readerPages.Count==0)return;
            readerBody.text=readerPages[readerPage];
            readerStatus.text=(readerPage+1)+" / "+readerPages.Count+" · "+T(reportView?D.ResearchGateClosed:D.UnverifiedPreview)+
                (readerTruncated?" · "+T(D.TextTruncated):"");
        }

        private void ShowPdf()
        {
            if(!readerOpen||readerItem==null||pdfBusy)return;
            if(!VitqraResearchPackage.Available){readerStatus.text=T(D.PackageUnavailable);return;}
            StartCoroutine(RenderPdfPage(0));
        }
        private IEnumerator RenderPdfPage(int index)
        {
            if(pdfBusy)yield break;
            var item=readerItem;
            int revision=readerRevision;
            var content=VitqraResearchPackage.Build(EvidenceRoot,ResultRoot,ReportRoot,DraftRoot,item,locale);
            if(content==null){readerStatus.text=T(D.PackageUnavailable);yield break;}
            pdfBusy=true;readerStatus.text=T(D.ReaderLoading);
            var cancel=new CancellationTokenSource();pdfCancel=cancel;
            Task<VitqraResearchPackage.Page> task=VitqraResearchPackage.Render(PackageRoot,content,index,cancel.Token);
            while(!task.IsCompleted)yield return null;
            if(ReferenceEquals(pdfCancel,cancel))pdfCancel=null;
            cancel.Dispose();
            if(!readerOpen||revision!=readerRevision)yield break;
            pdfBusy=false;
            var page=task.Status==TaskStatus.RanToCompletion?task.Result:null;
            if(page==null){readerStatus.text=T(D.PackageUnavailable);yield break;}
            var texture=new Texture2D(2,2,TextureFormat.RGBA32,false);
            if(!texture.LoadImage(page.png,true)){Destroy(texture);readerStatus.text=T(D.PackageUnavailable);yield break;}
            if(pdfTexture!=null)Destroy(pdfTexture);
            pdfTexture=texture;pdfImage.texture=texture;
            pdfView=true;pdfIndex=index;pdfCount=page.count;
            readerHeading.text=T(D.ViewPdf);
            readerBody.gameObject.SetActive(false);pdfImage.gameObject.SetActive(true);
            readerStatus.text=(pdfIndex+1)+" / "+pdfCount+" · "+T(D.ResearchGateClosed);
        }
        private void TurnReaderPage(int offset)
        {
            if(pdfView)
            {
                if(!pdfBusy)StartCoroutine(RenderPdfPage(Mathf.Clamp(pdfIndex+offset,0,pdfCount-1)));
                return;
            }
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
            if(pdfCancel!=null){pdfCancel.Cancel();pdfCancel=null;}
            pdfBusy=false;pdfView=false;
            if(pdfImage!=null){pdfImage.texture=null;pdfImage.gameObject.SetActive(false);}
            if(pdfTexture!=null){Destroy(pdfTexture);pdfTexture=null;}
            if(readerBody!=null)readerBody.gameObject.SetActive(true);
            if(readerRequest!=null)readerRequest.Abort();
            if(readerRoutine!=null){StopCoroutine(readerRoutine);readerRoutine=null;}
            readerRequest=null;readerOpen=false;readerItem=null;reportView=false;
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
