using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using C = Vitqra.VitqraChatStrings.Key;

namespace Vitqra
{
    public sealed class VitqraConversationPanel : VitqraSpatialPanel
    {
        public const float ChatWidth = .86f;
        public const float ChatHeight = .90f;
        private const int Rows = 4;
        private const int MaxCharacters = 1000;
        private const int MaxMessages = 200;
        private const int ChunkElements = 90;

        private sealed class Message
        {
            public bool user, sample;
            public string content;
        }
        private sealed class Fragment
        {
            public Message message;
            public string content;
        }
        private readonly List<Message> messages = new List<Message>();
        private readonly List<Fragment> fragments = new List<Fragment>();
        private readonly List<Rect> targets = new List<Rect>();
        private readonly List<Action> actions = new List<Action>();
        private readonly Text[] bubbles = new Text[Rows];
        private readonly Image[] bubbleBackgrounds = new Image[Rows];
        private Image background;
        private Text draftLabel, status, pageLabel;
        private string language, draft = "", editBuffer = "";
        private TouchScreenKeyboard keyboard;
        private bool desktopEditing;
        private int first;
        private Action minimize;
        public event Action<string> MessageSubmitted;
        public event Action VoiceRequested;
        private string T(C key) => VitqraChatStrings.Get(language,key);

        public void InitializeChat(string locale, Action close)
        {
            language = locale;
            minimize = close;
            var obj = new GameObject("Conversation Canvas", typeof(RectTransform), typeof(Canvas));
            var rect = (RectTransform)obj.transform;
            rect.SetParent(transform,false);
            rect.sizeDelta = new Vector2(860,900);
            rect.localScale = Vector3.one * .001f;
            obj.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            background = Box("Background",rect,Vector2.zero,new Vector2(860,900),new Color(.025f,.065f,.10f,.98f));
            Box("Accent",rect,new Vector2(0,446),new Vector2(860,8),new Color(.24f,.95f,.92f));
            Label("Heading",rect,new Vector2(-100,402),new Vector2(610,52),T(C.Title),34,Color.cyan);
            AddButton(rect,310,402,200,52,T(C.Minimize),() => minimize?.Invoke());
            Label("Privacy",rect,new Vector2(0,350),new Vector2(810,40),T(C.Privacy),22,Color.white);
            for (int i=0;i<Rows;i++)
            {
                float y=274-i*126;
                bubbleBackgrounds[i]=Box("Bubble "+i,rect,new Vector2(0,y),new Vector2(810,118),new Color(.08f,.15f,.22f,1));
                bubbles[i]=Label("Message "+i,rect,new Vector2(0,y),new Vector2(780,110),"",23,Color.white);
                bubbles[i].alignment=TextAnchor.MiddleLeft;
            }
            AddButton(rect,-280,-174,240,48,T(C.Older),()=>{first=Mathf.Max(0,first-Rows);RenderHistory();});
            AddButton(rect,280,-174,240,48,T(C.Newer),()=>{first=Mathf.Min(MaxFirst,first+Rows);RenderHistory();});
            pageLabel=Label("Page",rect,new Vector2(0,-174),new Vector2(285,48),"",22,Color.white);
            status=Label("Status",rect,new Vector2(0,-230),new Vector2(810,52),"",21,Color.white);
            Box("Composer",rect,new Vector2(0,-308),new Vector2(810,88),new Color(.06f,.14f,.20f,1));
            draftLabel=Label("Draft",rect,new Vector2(0,-308),new Vector2(775,80),T(C.Prompt),23,Color.white);
            draftLabel.alignment=TextAnchor.MiddleLeft;
            targets.Add(new Rect(-.405f,-.352f,.81f,.088f));
            actions.Add(EditDraft);
            AddButton(rect,-270,-405,250,58,T(C.Write),EditDraft);
            AddButton(rect,0,-405,250,58,T(C.Voice),RequestVoice);
            AddButton(rect,270,-405,250,58,T(C.Send),Send);
            RenderHistory();
        }

        private void AddButton(RectTransform parent,float x,float y,float width,float height,string title,Action callback)
        {
            Box(title,parent,new Vector2(x,y),new Vector2(width,height),new Color(.10f,.27f,.31f,1));
            Label(title+" text",parent,new Vector2(x,y),new Vector2(width-10,height-6),title,23,Color.white);
            targets.Add(new Rect((x-width*.5f)*.001f,(y-height*.5f)*.001f,width*.001f,height*.001f));
            actions.Add(callback);
        }

        public override void Highlight(bool value)
        {
            if (background!=null) background.color=value?new Color(.07f,.14f,.20f,.98f):new Color(.025f,.065f,.10f,.98f);
        }
        public override void InvokeAction(int region)
        {
            if (region>0 && region<=actions.Count) actions[region-1]?.Invoke();
        }
        public override bool Intersect(Ray ray,out float distance,out int actionRegion)
        {
            distance=0; actionRegion=0;
            if (!gameObject.activeInHierarchy || Vector3.Dot(ray.direction,transform.forward)<=.0001f) return false;
            if (!new Plane(transform.forward,transform.position).Raycast(ray,out distance)||distance>4f) return false;
            Vector3 local=transform.InverseTransformPoint(ray.GetPoint(distance));
            if (Mathf.Abs(local.x)>ChatWidth*.5f || Mathf.Abs(local.y)>ChatHeight*.5f) return false;
            for(int i=0;i<targets.Count;i++)
                if(targets[i].Contains(new Vector2(local.x,local.y))){actionRegion=i+1;break;}
            return true;
        }

        public void AddAssistantMessage(string content,bool sample=false)
        {
            AddMessage(false,content,sample);
        }
        private void AddMessage(bool user,string content,bool sample=false)
        {
            if(string.IsNullOrWhiteSpace(content)) return;
            messages.Add(new Message {user=user,sample=sample,content=content});
            if(messages.Count>MaxMessages) messages.RemoveAt(0);
            RebuildFragments();
            first=MaxFirst;
            RenderHistory();
        }
        private int MaxFirst => Mathf.Max(0,fragments.Count-Rows);
        private void RebuildFragments()
        {
            fragments.Clear();
            foreach(var m in messages)
            {
                var indices=StringInfo.ParseCombiningCharacters(m.content);
                for(int i=0;i<indices.Length;i+=ChunkElements)
                {
                    int end=Mathf.Min(i+ChunkElements,indices.Length);
                    int stop=end==indices.Length?m.content.Length:indices[end];
                    fragments.Add(new Fragment {message=m,content=m.content.Substring(indices[i],stop-indices[i])});
                }
            }
        }
        private void RenderHistory()
        {
            first=Mathf.Clamp(first,0,MaxFirst);
            for(int i=0;i<Rows;i++)
            {
                int index=first+i;
                bool visible=index<fragments.Count;
                bubbleBackgrounds[i].gameObject.SetActive(visible);
                bubbles[i].gameObject.SetActive(visible || (i==0 && fragments.Count==0));
                if(!visible){bubbles[i].text=i==0?T(C.Empty):"";continue;}
                var f=fragments[index];
                var m=f.message;
                bubbles[i].alignment=m.user?TextAnchor.MiddleRight:TextAnchor.MiddleLeft;
                bubbleBackgrounds[i].color=m.user?new Color(.12f,.30f,.34f,1):new Color(.10f,.15f,.26f,1);
                bubbles[i].text=T(m.user?C.You:m.sample?C.PreviewTip:C.Assistant)+"\n"+f.content;
            }
            pageLabel.text=fragments.Count==0?"0 / 0":(first+1)+"–"+Mathf.Min(first+Rows,fragments.Count)+" / "+fragments.Count;
        }

        private void SetDraft(string value)
        {
            draft=value??"";
            if(draft.Length>MaxCharacters)
            {
                int length=MaxCharacters;
                if(char.IsHighSurrogate(draft[length-1])) length--;
                draft=draft.Substring(0,length);
            }
            var indices=StringInfo.ParseCombiningCharacters(draft);
            draftLabel.text=draft.Length==0?T(C.Prompt):indices.Length<=120?draft:"…"+draft.Substring(indices[indices.Length-120]);
        }

        private void EditDraft()
        {
            if(keyboard!=null||desktopEditing) return;
            if(TouchScreenKeyboard.isSupported)
            {
                keyboard=TouchScreenKeyboard.Open(draft,TouchScreenKeyboardType.Default,true,true,false,false,T(C.Prompt),MaxCharacters);
                if(keyboard!=null) return;
            }
#if UNITY_EDITOR || UNITY_STANDALONE
            editBuffer=draft;
            desktopEditing=true;
#else
            status.text=T(C.Prompt);
#endif
        }
        private void Update()
        {
            if(keyboard==null) return;
            if(keyboard.status==TouchScreenKeyboard.Status.Done)
            {
                SetDraft(keyboard.text);
                keyboard=null;
            }
            else if(keyboard.status==TouchScreenKeyboard.Status.Canceled || keyboard.status==TouchScreenKeyboard.Status.LostFocus)
                keyboard=null;
        }
        private void OnGUI()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if(!desktopEditing) return;
            GUILayout.BeginArea(new Rect(20,20,Mathf.Max(100,Mathf.Min(600,Screen.width-40)),240),GUI.skin.box);
            GUILayout.Label(T(C.Prompt));
            editBuffer=GUILayout.TextArea(editBuffer,MaxCharacters,GUILayout.Height(120));
            if(GUILayout.Button(T(C.Done))){SetDraft(editBuffer);desktopEditing=false;}
            if(GUILayout.Button(T(C.Cancel)))desktopEditing=false;
            GUILayout.EndArea();
#endif
        }

        private void RequestVoice()
        {
            if(VoiceRequested==null){status.text=T(C.NoVoice);return;}
            VoiceRequested.Invoke();
        }

        // Future speech engine calls this on Unity's main thread only after explicit activation.
        public void ApplyVoiceTranscript(string text)
        {
            if(!gameObject.activeInHierarchy || string.IsNullOrWhiteSpace(text)) return;
            SetDraft(string.IsNullOrWhiteSpace(draft)?text:draft+" "+text);
        }

        private void Send()
        {
            if(keyboard!=null||desktopEditing||string.IsNullOrWhiteSpace(draft))return;
            string message=draft.Trim();
            AddMessage(true,message);
            SetDraft("");
            if(MessageSubmitted==null){status.text=T(C.NoAi);return;}
            status.text="";
            MessageSubmitted.Invoke(message);
        }

        private void OnDisable()
        {
            if(keyboard!=null){keyboard.active=false;keyboard=null;}
            desktopEditing=false;
        }
    }
}
