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
        private Image localBox,chatBox;
        private Text status,translation;
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
            Button(rect,-150,82,290,48,T(K.Local),()=>select("local"),out localBox);
            Button(rect,150,82,290,48,T(K.ChatGpt),()=>select("chatgpt"),out chatBox);
            status=Label("Provider state",rect,new Vector2(0,23),new Vector2(594,64),"",19,Color.white);
            status.supportRichText=false;
            Label("Global provider",rect,new Vector2(0,-21),new Vector2(590,24),T(K.Global),16,Color.cyan);
            translation=Button(rect,0,-62,590,44,"",toggleTranslation,out _);
            Button(rect,0,-125,590,48,T(K.Avatar),avatar,out _);
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
            chatBox.color=local?NormalButton:Selected;
            status.text=T(local?(modelReady?K.Ready:K.NotReady):K.Pending);
            translation.text=T(K.Translate)+": "+T(settings.automaticTranslation?K.On:K.Off);
        }
        public override bool Intersect(Ray ray,out float distance,out int region)
        {
            distance=0;region=0;
            if(!gameObject.activeInHierarchy||Vector3.Dot(ray.direction,transform.forward)<=.0001f||
               !new Plane(transform.forward,transform.position).Raycast(ray,out distance)||distance>4f)return false;
            Vector3 local=transform.InverseTransformPoint(ray.GetPoint(distance));
            if(Mathf.Abs(local.x)>Width*.5f||Mathf.Abs(local.y)>Height*.5f)return false;
            for(int i=0;i<targets.Count;i++)if(targets[i].Contains(new Vector2(local.x,local.y))){region=i+1;break;}
            return true;
        }
        public override void InvokeAction(int region){if(region>0&&region<=actions.Count)actions[region-1]?.Invoke();}
    }
}
