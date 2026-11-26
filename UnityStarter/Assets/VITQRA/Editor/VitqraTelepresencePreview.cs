using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using D=Vitqra.VitqraCommunityPolicy;
using T=Vitqra.VitqraTelepresence;
namespace Vitqra
{
    public sealed class VitqraTelepresencePreview:EditorWindow
    {
        private T session;
        private int viewer;
        private readonly int[] sample={0,0},sequence={0,0};
        private readonly string[] status={"",""};
        private static readonly Dictionary<string,string[]> Labels=new Dictionary<string,string[]>
        {
            {"en",new[]{"Synthetic telepresence","Fictional poses only. No hardware tracking, call or recording.","New session","Access day","Elapsed milliseconds","Participant A","Participant B","Join","Leave","Share fictional head","Share fictional hands","Sample","Neutral","Turn","Left gesture","Right gesture","Head tracking lost","Hand tracking lost","Invalid coordinates","Submit sample","View from","No visible pose","Head / left / right","Advance 500 ms"}},
            {"ca",new[]{"Telepresència sintètica","Només poses fictícies. Sense seguiment real, trucada ni enregistrament.","Nova sessió","Dia d'accés","Mil·lisegons transcorreguts","Participant A","Participant B","Entra","Surt","Comparteix cap fictici","Comparteix mans fictícies","Mostra","Neutra","Gir","Gest esquerre","Gest dret","Seguiment del cap perdut","Seguiment de mans perdut","Coordenades incorrectes","Envia mostra","Vista des de","Cap posa visible","Cap / esquerra / dreta","Avança 500 ms"}},
            {"es",new[]{"Telepresencia sintética","Solo poses ficticias. Sin seguimiento real, llamada ni grabación.","Nueva sesión","Día de acceso","Milisegundos transcurridos","Participante A","Participante B","Entrar","Salir","Compartir cabeza ficticia","Compartir manos ficticias","Muestra","Neutra","Giro","Gesto izquierdo","Gesto derecho","Seguimiento de cabeza perdido","Seguimiento de manos perdido","Coordenadas incorrectas","Enviar muestra","Vista desde","Sin pose visible","Cabeza / izquierda / derecha","Avanzar 500 ms"}},
            {"zh",new[]{"合成远程临场","仅虚构姿态。无硬件追踪、通话或录制。","新会话","访问日","经过毫秒","参与者 A","参与者 B","加入","离开","分享虚构头部","分享虚构双手","样本","中立","转头","左手动作","右手动作","头部追踪丢失","手部追踪丢失","无效坐标","提交样本","观察者","无可见姿态","头 / 左 / 右","前进 500 毫秒"}},
            {"hi",new[]{"कृत्रिम दूरस्थ उपस्थिति","केवल काल्पनिक मुद्राएँ। वास्तविक ट्रैकिंग, कॉल या रिकॉर्डिंग नहीं।","नया सत्र","पहुँच दिन","बीते मिलीसेकंड","भागीदार A","भागीदार B","जुड़ें","छोड़ें","काल्पनिक सिर साझा करें","काल्पनिक हाथ साझा करें","नमूना","तटस्थ","मोड़","बायाँ इशारा","दायाँ इशारा","सिर ट्रैकिंग खो गई","हाथ ट्रैकिंग खो गई","अमान्य निर्देशांक","नमूना भेजें","दृश्य","कोई दृश्य मुद्रा नहीं","सिर / बायाँ / दायाँ","500 मिलीसेकंड आगे"}},
            {"fr",new[]{"Téléprésence synthétique","Poses fictives uniquement. Aucun suivi matériel, appel ou enregistrement.","Nouvelle session","Jour d’accès","Millisecondes écoulées","Participant A","Participant B","Entrer","Quitter","Partager la tête fictive","Partager les mains fictives","Échantillon","Neutre","Rotation","Geste gauche","Geste droit","Suivi tête perdu","Suivi mains perdu","Coordonnées invalides","Envoyer l’échantillon","Vue depuis","Aucune pose visible","Tête / gauche / droite","Avancer de 500 ms"}},
            {"de",new[]{"Synthetische Telepräsenz","Nur fiktive Posen. Kein Hardware-Tracking, Anruf oder Mitschnitt.","Neue Sitzung","Zugriffstag","Vergangene Millisekunden","Teilnehmer A","Teilnehmer B","Beitreten","Verlassen","Fiktiven Kopf teilen","Fiktive Hände teilen","Probe","Neutral","Drehung","Linke Geste","Rechte Geste","Kopftracking verloren","Handtracking verloren","Ungültige Koordinaten","Probe senden","Ansicht von","Keine sichtbare Pose","Kopf / links / rechts","500 ms weiter"}},
            {"pt",new[]{"Telepresença sintética","Apenas poses fictícias. Sem rastreio real, chamada ou gravação.","Nova sessão","Dia de acesso","Milissegundos decorridos","Participante A","Participante B","Entrar","Sair","Partilhar cabeça fictícia","Partilhar mãos fictícias","Amostra","Neutra","Rotação","Gesto esquerdo","Gesto direito","Rastreio da cabeça perdido","Rastreio das mãos perdido","Coordenadas inválidas","Enviar amostra","Vista de","Sem pose visível","Cabeça / esquerda / direita","Avançar 500 ms"}},
            {"ar",new[]{"حضور عن بعد اصطناعي","وضعيات خيالية فقط. لا تتبع حقيقي أو اتصال أو تسجيل.","جلسة جديدة","يوم الوصول","المللي ثانية المنقضية","المشارك A","المشارك B","انضم","غادر","مشاركة الرأس الخيالي","مشاركة اليدين الخياليتين","عينة","محايد","دوران","إشارة يسار","إشارة يمين","فقد تتبع الرأس","فقد تتبع اليدين","إحداثيات غير صالحة","إرسال عينة","عرض من","لا وضعية مرئية","رأس / يسار / يمين","تقدم 500 مللي ثانية"}},
            {"ja",new[]{"合成テレプレゼンス","架空の姿勢のみ。実機追跡、通話、録音なし。","新しいセッション","アクセス日","経過ミリ秒","参加者 A","参加者 B","参加","退出","架空の頭部を共有","架空の手を共有","サンプル","中立","回転","左手ジェスチャー","右手ジェスチャー","頭部追跡喪失","手の追跡喪失","無効な座標","サンプル送信","視点","表示可能な姿勢なし","頭 / 左 / 右","500 ms 進める"}},
        };
        [MenuItem("VITQRA/Synthetic Telepresence Preview")]
        private static void Open()=>GetWindow<VitqraTelepresencePreview>();
        private static D.Actor Actor(int i)=>i==0?D.Actor.PatientA:D.Actor.ClinicianA;
        private void ResetSession()
        {
            session?.Clear();session=new T(D.Resource.PrivateRoomA);
            for(int i=0;i<2;i++){sequence[i]=0;sample[i]=0;status[i]="";}
        }
        private void OnGUI()
        {
            string[] l;if(!Labels.TryGetValue(VitqraStrings.DeviceLanguage,out l))l=Labels["en"];
            titleContent=new GUIContent(l[0]);EditorGUILayout.HelpBox(l[1],MessageType.Warning);
            if(session==null)ResetSession();
            if(GUILayout.Button(l[2]))ResetSession();
            session.AdvanceDay(EditorGUILayout.IntSlider(l[3],session.Day,session.Day,180));
            EditorGUILayout.LabelField(l[4],session.Now.ToString());
            if(GUILayout.Button(l[23]))session.AdvanceMs(Math.Min(T.MaxTimeMs,session.Now+500));
            for(int i=0;i<2;i++)
            {
                var a=Actor(i);EditorGUILayout.LabelField(l[5+i],EditorStyles.boldLabel);
                if(!session.Joined(a)){if(GUILayout.Button(l[7])&&session.Join(a,i)){sequence[i]=0;status[i]="";}}
                else
                {
                    if(GUILayout.Button(l[8]))session.Leave(a);
                    if(session.Joined(a))
                    {
                        bool h=EditorGUILayout.Toggle(l[9],session.HeadConsent(a));
                        bool k=EditorGUILayout.Toggle(l[10],session.HandConsent(a));
                        session.SetConsent(a,h,k);
                        sample[i]=EditorGUILayout.Popup(l[11],sample[i],new[]{l[12],l[13],l[14],l[15],l[16],l[17],l[18]});
                        if(GUILayout.Button(l[19]))status[i]=session.SubmitFixture(a,(T.Sample)sample[i],sequence[i]++,session.Now).ToString();
                        EditorGUILayout.LabelField(status[i]);
                    }
                }
            }
            viewer=EditorGUILayout.Popup(l[20],viewer,new[]{l[5],l[6]});
            var p=session.Read(Actor(viewer),Actor(1-viewer));
            Rect r=GUILayoutUtility.GetRect(160,180,GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(r,new Color(.06f,.10f,.16f));
            if(p==null){EditorGUILayout.LabelField(l[21]);return;}
            DrawPoint(r,p.head,Color.cyan,16);
            DrawPoint(r,p.left,new Color(.75f,.64f,1),10);
            DrawPoint(r,p.right,new Color(.75f,.64f,1),10);
            if(p.head!=null)
            {
                Vector2 center=Project(r,p.head);
                float angle=p.head.yaw*Mathf.Deg2Rad;
                Handles.BeginGUI();Handles.color=Color.cyan;
                Handles.DrawLine(center,center+new Vector2(Mathf.Sin(angle)*26,-Mathf.Cos(angle)*26));
                Handles.EndGUI();
            }
            EditorGUILayout.LabelField(l[22],(p.head!=null?"1":"0")+" / "+(p.left!=null?"1":"0")+" / "+(p.right!=null?"1":"0"));
        }
        private static Vector2 Project(Rect r,T.Point p)=>new Vector2(r.center.x+p.x*r.width/4400f,r.yMax-8-p.y*(r.height-16)/2500f);
        private static void DrawPoint(Rect r,T.Point p,Color c,float size)
        {if(p==null)return;var q=Project(r,p);EditorGUI.DrawRect(new Rect(q.x-size/2,q.y-size/2,size,size),c);}
        private void OnDisable(){session?.Clear();session=null;}
    }
}
