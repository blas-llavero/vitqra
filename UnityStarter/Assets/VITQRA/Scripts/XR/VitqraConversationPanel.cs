using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using C = Vitqra.VitqraChatStrings.Key;

namespace Vitqra
{
    public sealed class VitqraConversationPanel : VitqraSpatialPanel
    {
        public const float ChatWidth = .86f;
        public const float ChatHeight = .90f;
        public override float BaseWidth => ChatWidth;
        public override float BaseHeight => ChatHeight;
        private const int Rows = 4;
        private const int MaxCharacters = 1000;
        private const int MaxMessages = 200;
        private const int ChunkElements = 72;

        [Serializable]
        private sealed class Message
        {
            public bool user, sample;
            public string content;
        }
        [Serializable]
        private sealed class Archive
        {
            public int version = 1;
            public List<Message> messages = new List<Message>();
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
        private Text draftLabel, status, pageLabel, storageLabel, saveButton, deleteButton, voiceButton;
        private bool persistHistory, deleteArmed;
        private string archivePath;
        private const string HistoryPreference = "vitqra.chat.persist.v1";
        private const int MaxArchiveBytes = 8 * 1024 * 1024;
        private const int MaxMessageLength = 8000;
        public int MessageCount => messages.Count;
        public event Action VoiceCanceled;
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
            storageLabel=Label("Storage mode",rect,new Vector2(0,312),new Vector2(810,34),"",19,Color.white);
            saveButton=AddButton(rect,-205,275,390,36,T(C.SaveLocal),TogglePersistence);
            deleteButton=AddButton(rect,205,275,390,36,T(C.DeleteHistory),DeleteHistory);
            for (int i=0;i<Rows;i++)
            {
                float y=196-i*103;
                bubbleBackgrounds[i]=Box("Bubble "+i,rect,new Vector2(0,y),new Vector2(810,97),new Color(.08f,.15f,.22f,1));
                bubbles[i]=Label("Message "+i,rect,new Vector2(0,y),new Vector2(780,92),"",23,Color.white);
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
            voiceButton=AddButton(rect,0,-405,250,58,T(C.Voice),RequestVoice);
            AddButton(rect,270,-405,250,58,T(C.Send),Send);
            LoadHistory();
            UpdateStorageLabels();
            RenderHistory();
        }

        private Text AddButton(RectTransform parent,float x,float y,float width,float height,string title,Action callback)
        {
            Box(title,parent,new Vector2(x,y),new Vector2(width,height),new Color(.10f,.27f,.31f,1));
            var label=Label(title+" text",parent,new Vector2(x,y),new Vector2(width-10,height-6),title,23,Color.white);
            targets.Add(new Rect((x-width*.5f)*.001f,(y-height*.5f)*.001f,width*.001f,height*.001f));
            actions.Add(callback);
            return label;
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
            if(content.Length>MaxMessageLength)
            {
                int limit=MaxMessageLength;
                if(char.IsHighSurrogate(content[limit-1])) limit--;
                content=content.Substring(0,limit)+"…";
            }
            messages.Add(new Message {user=user,sample=sample,content=content});
            if(messages.Count>MaxMessages) messages.RemoveAt(0);
            RebuildFragments();
            first=MaxFirst;
            RenderHistory();
            if(persistHistory) SaveHistory();
        }

        private void LoadHistory()
        {
            archivePath=Path.Combine(Application.persistentDataPath,"vitqra-chat-v1.json");
            persistHistory=PlayerPrefs.GetInt(HistoryPreference,0)==1;
            if(!persistHistory) return;
            try
            {
                if(!File.Exists(archivePath)) return;
                if(new FileInfo(archivePath).Length>MaxArchiveBytes) throw new IOException();
                var archive=JsonUtility.FromJson<Archive>(File.ReadAllText(archivePath));
                if(archive==null||archive.version!=1||archive.messages==null) throw new IOException();
                int start=Math.Max(0,archive.messages.Count-MaxMessages);
                for(int i=start;i<archive.messages.Count;i++)
                {
                    var m=archive.messages[i];
                    if(m!=null&&!string.IsNullOrWhiteSpace(m.content)&&m.content.Length<=MaxMessageLength+1) messages.Add(m);
                }
                RebuildFragments();
                first=MaxFirst;
            }
            catch(Exception)
            {
                persistHistory=false;
                PlayerPrefs.SetInt(HistoryPreference,0);
                PlayerPrefs.Save();
                status.text=T(C.StorageError);
            }
        }

        private bool SaveHistory()
        {
            string temporary=archivePath+".tmp";
            try
            {
                var json=JsonUtility.ToJson(new Archive {messages=messages});
                if(System.Text.Encoding.UTF8.GetByteCount(json)>MaxArchiveBytes) throw new IOException();
                File.WriteAllText(temporary,json);
                if(File.Exists(archivePath)) File.Replace(temporary,archivePath,null);
                else File.Move(temporary,archivePath);
                return true;
            }
            catch(Exception)
            {
                try { File.Delete(temporary); } catch(Exception) {}
                status.text=T(C.StorageError);
                return false;
            }
        }

        private bool RemoveArchive()
        {
            try
            {
                File.Delete(archivePath);
                File.Delete(archivePath+".tmp");
                return true;
            }
            catch(Exception)
            {
                status.text=T(C.StorageError);
                return false;
            }
        }

        private void UpdateStorageLabels()
        {
            storageLabel.text=T(persistHistory?C.LocalHistory:C.SessionOnly);
            saveButton.text=T(persistHistory?C.StopSaving:C.SaveLocal);
            deleteButton.text=T(deleteArmed?C.ConfirmDelete:C.DeleteHistory);
        }

        private void TogglePersistence()
        {
            deleteArmed=false;
            if(persistHistory)
            {
                if(!RemoveArchive()){UpdateStorageLabels();return;}
                persistHistory=false;
            }
            else
            {
                // Never silently overwrite an archive left behind by a failed read or deletion.
                if(File.Exists(archivePath)||File.Exists(archivePath+".tmp"))
                {status.text=T(C.DeleteSavedFirst);UpdateStorageLabels();return;}
                if(!SaveHistory()){UpdateStorageLabels();return;}
                persistHistory=true;
            }
            PlayerPrefs.SetInt(HistoryPreference,persistHistory?1:0);
            PlayerPrefs.Save();
            status.text="";
            UpdateStorageLabels();
        }

        private void DeleteHistory()
        {
            if(!deleteArmed){deleteArmed=true;UpdateStorageLabels();return;}
            deleteArmed=false;
            if(RemoveArchive())
            {
                messages.Clear();
                fragments.Clear();
                first=0;
                persistHistory=false;
                PlayerPrefs.SetInt(HistoryPreference,0);
                PlayerPrefs.Save();
                RenderHistory();
                status.text="";
            }
            UpdateStorageLabels();
        }

        public void SetVoiceState(bool recording,bool processing=false,bool failed=false)
        {
            voiceButton.text=T(recording?C.StopRecording:C.Voice);
            status.text=failed?T(C.VoiceFailed):recording?T(C.Recording):processing?T(C.VoiceProcessing):"";
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
            if(MessageSubmitted==null){status.text=T(C.NotSent);return;}
            status.text="";
            MessageSubmitted.Invoke(message);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if(keyboard!=null){keyboard.active=false;keyboard=null;}
            desktopEditing=false;
            deleteArmed=false;
            if(deleteButton!=null) UpdateStorageLabels();
            VoiceCanceled?.Invoke();
        }
    }
}
