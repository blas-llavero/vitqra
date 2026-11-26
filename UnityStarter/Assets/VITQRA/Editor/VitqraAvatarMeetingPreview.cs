using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using D=Vitqra.VitqraCommunityPolicy;
using M=Vitqra.VitqraAvatarMeeting;

namespace Vitqra
{
    public sealed class VitqraAvatarMeetingPreview:EditorWindow
    {
        private M meeting;
        private int room,activeRoom;
        private readonly int[] profiles={0,1};
        private GameObject visual;
        private VitqraAvatarPreview[] avatars;
        private static readonly Dictionary<string,string[]> Labels=new Dictionary<string,string[]>
        {
            {"en",new[]{"Local avatar meeting","Fictional room only. No real call, microphone or recording.","Room","New session","Access day","Elapsed seconds","Participant A","Participant B","Join","Leave","Muted","Profile","Man","Woman","Boy","Girl","Greeting","Ready","Thank you","Caption","Hello.","I am ready.","Thank you.","No active caption","3D preview (Play mode)","Close 3D preview","Not joined"}},
            {"ca",new[]{"Reunió local amb avatars","Sala fictícia. Sense trucada, micròfon ni enregistrament real.","Sala","Nova sessió","Dia d'accés","Segons transcorreguts","Participant A","Participant B","Entra","Surt","Silenciat","Perfil","Home","Dona","Noi","Noia","Salutació","Preparat","Agraïment","Subtítol","Hola.","Tot a punt.","Gràcies.","Sense subtítol actiu","Vista 3D (mode Play)","Tanca vista 3D","Fora de la sala"}},
            {"es",new[]{"Reunión local con avatares","Sala ficticia. Sin llamada, micrófono ni grabación real.","Sala","Nueva sesión","Día de acceso","Segundos transcurridos","Participante A","Participante B","Entrar","Salir","Silenciado","Perfil","Hombre","Mujer","Chico","Chica","Saludo","Preparado","Agradecimiento","Subtítulo","Hola.","Todo listo.","Gracias.","Sin subtítulo activo","Vista 3D (modo Play)","Cerrar vista 3D","Fuera de la sala"}},
            {"zh",new[]{"本地虚拟形象会议","虚构房间。无真实通话、麦克风或录音。","房间","新会话","访问日","经过秒数","参与者 A","参与者 B","加入","离开","静音","形象","成年男性","成年女性","男孩","女孩","问候","准备","感谢","字幕","你好。","准备好了。","谢谢。","无当前字幕","3D 预览（播放模式）","关闭 3D 预览","未加入"}},
            {"hi",new[]{"स्थानीय अवतार बैठक","काल्पनिक कक्ष। वास्तविक कॉल, माइक्रोफोन या रिकॉर्डिंग नहीं।","कक्ष","नया सत्र","पहुँच दिन","बीते सेकंड","भागीदार A","भागीदार B","जुड़ें","छोड़ें","मौन","प्रोफ़ाइल","पुरुष","महिला","लड़का","लड़की","अभिवादन","तैयार","धन्यवाद","उपशीर्षक","नमस्ते।","तैयारी पूरी है।","धन्यवाद।","सक्रिय उपशीर्षक नहीं","3D पूर्वावलोकन (Play मोड)","3D पूर्वावलोकन बंद करें","नहीं जुड़े"}},
            {"fr",new[]{"Réunion locale avec avatars","Salle fictive. Aucun appel, microphone ou enregistrement réel.","Salle","Nouvelle session","Jour d’accès","Secondes écoulées","Participant A","Participant B","Entrer","Quitter","Muet","Profil","Homme","Femme","Garçon","Fille","Salutation","Prêt","Remerciement","Sous-titre","Bonjour.","Tout est prêt.","Merci.","Aucun sous-titre actif","Aperçu 3D (mode Play)","Fermer l’aperçu 3D","Hors de la salle"}},
            {"de",new[]{"Lokales Avatar-Treffen","Fiktiver Raum. Kein echter Anruf, Mikrofon oder Mitschnitt.","Raum","Neue Sitzung","Zugriffstag","Vergangene Sekunden","Teilnehmer A","Teilnehmer B","Beitreten","Verlassen","Stumm","Profil","Mann","Frau","Junge","Mädchen","Begrüßung","Bereit","Dank","Untertitel","Hallo.","Alles bereit.","Danke.","Kein aktiver Untertitel","3D-Vorschau (Play-Modus)","3D-Vorschau schließen","Nicht beigetreten"}},
            {"pt",new[]{"Reunião local com avatares","Sala fictícia. Sem chamada, microfone ou gravação real.","Sala","Nova sessão","Dia de acesso","Segundos decorridos","Participante A","Participante B","Entrar","Sair","Silenciado","Perfil","Homem","Mulher","Rapaz","Rapariga","Saudação","Pronto","Agradecimento","Legenda","Olá.","Tudo pronto.","Obrigado.","Sem legenda ativa","Vista 3D (modo Play)","Fechar vista 3D","Fora da sala"}},
            {"ar",new[]{"اجتماع محلي بالصور الرمزية","غرفة خيالية. لا اتصال أو ميكروفون أو تسجيل حقيقي.","الغرفة","جلسة جديدة","يوم الوصول","الثواني المنقضية","المشارك A","المشارك B","انضم","غادر","صامت","الملف","رجل","امرأة","ولد","فتاة","تحية","استعداد","شكر","ترجمة نصية","مرحبًا.","كل شيء جاهز.","شكرًا.","لا نص نشط","معاينة ثلاثية الأبعاد (وضع Play)","إغلاق المعاينة","غير منضم"}},
            {"ja",new[]{"ローカルアバター会議","架空の部屋。実際の通話、マイク、録音なし。","部屋","新しいセッション","アクセス日","経過秒数","参加者 A","参加者 B","参加","退出","ミュート","プロフィール","男性","女性","少年","少女","挨拶","準備","感謝","字幕","こんにちは。","準備できました。","ありがとう。","現在の字幕なし","3D プレビュー（Play モード）","3D プレビューを閉じる","未参加"}},
        };
        [MenuItem("VITQRA/Synthetic Avatar Meeting Preview")]
        private static void Open()=>GetWindow<VitqraAvatarMeetingPreview>();
        private D.Actor Actor(int i)=>activeRoom==0?(i==0?D.Actor.PatientA:D.Actor.ClinicianA):(i==0?D.Actor.PatientB:D.Actor.ClinicianB);
        private void ResetSession()
        {
            meeting?.Clear();CloseVisual();activeRoom=room;
            meeting=new M(activeRoom==0?D.Resource.PrivateRoomA:D.Resource.PrivateRoomB);
        }
        private void OnGUI()
        {
            string[] l;if(!Labels.TryGetValue(VitqraStrings.DeviceLanguage,out l))l=Labels["en"];
            titleContent=new GUIContent(l[0]);EditorGUILayout.HelpBox(l[1],MessageType.Warning);
            if(meeting==null)ResetSession();
            room=EditorGUILayout.Popup(l[2],room,new[]{"A","B"});
            if(GUILayout.Button(l[3]))ResetSession();
            EditorGUILayout.LabelField(l[2],activeRoom==0?"A":"B");
            meeting.AdvanceDay(EditorGUILayout.IntSlider(l[4],meeting.Day,meeting.Day,180));
            meeting.AdvanceSecond(EditorGUILayout.IntSlider(l[5],meeting.Second,meeting.Second,86400));
            for(int i=0;i<2;i++)
            {
                var a=Actor(i);EditorGUILayout.LabelField(l[6+i],EditorStyles.boldLabel);
                profiles[i]=EditorGUILayout.Popup(l[11],profiles[i],new[]{l[12],l[13],l[14],l[15]});
                if(!meeting.Joined(a))
                {
                    EditorGUILayout.LabelField(l[26]);
                    if(GUILayout.Button(l[8]))meeting.Join(a,profiles[i]);
                }
                else
                {
                    meeting.SetProfile(a,profiles[i]);
                    if(GUILayout.Button(l[9]))meeting.Leave(a);
                    if(meeting.Joined(a))
                    {
                        meeting.SetMuted(a,EditorGUILayout.Toggle(l[10],meeting.Muted(a)));
                        using(new EditorGUI.DisabledScope(meeting.Muted(a)))
                            for(int p=0;p<3;p++)if(GUILayout.Button(l[16+p]))meeting.SpeakFixture(a,(M.Phrase)p);
                    }
                }
                var caption=meeting.ReadCaption(a);
                EditorGUILayout.LabelField(l[19],caption==null?l[23]:l[20+(int)caption.phrase]);
            }
            // Optional procedural display in an isolated empty Play-mode scene.
            // Never attach to the existing workspace or alter its saved layout.
            using(new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
                if(GUILayout.Button(l[24]))CreateVisual();
            if(GUILayout.Button(l[25]))CloseVisual();
            UpdateVisual();
        }
        private void CreateVisual()
        {
            CloseVisual();
            // This display is deliberately refused in a live VITQRA workspace.
            if(!EditorApplication.isPlaying||UnityEngine.Object.FindAnyObjectByType<VitqraSpatialWorkspace>()!=null)return;
            visual=new GameObject("VITQRA fictional meeting preview");
            visual.hideFlags=HideFlags.DontSave;
            avatars=new VitqraAvatarPreview[2];
            for(int i=0;i<2;i++)
            {
                var obj=new GameObject("Fictional participant "+i);obj.transform.SetParent(visual.transform,false);
                obj.transform.localPosition=new Vector3(i==0?-.25f:.25f,1.3f,1.5f);
                avatars[i]=obj.AddComponent<VitqraAvatarPreview>();avatars[i].SetProfile(-1);
            }
        }
        private void UpdateVisual()
        {
            if(visual==null||avatars==null)return;
            if(!EditorApplication.isPlaying||UnityEngine.Object.FindAnyObjectByType<VitqraSpatialWorkspace>()!=null){CloseVisual();return;}
            for(int i=0;i<2;i++)if(avatars[i]!=null)avatars[i].SetProfile(meeting.Profile(Actor(i)));
        }
        private void CloseVisual(){if(visual!=null)UnityEngine.Object.DestroyImmediate(visual);visual=null;avatars=null;}
        private void OnDisable(){meeting?.Clear();CloseVisual();}
    }
}
