using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using D=Vitqra.VitqraCommunityPolicy;

namespace Vitqra
{
    public sealed class VitqraCommunityPreview:EditorWindow
    {
        private D.Actor actor=D.Actor.ClinicianA;
        private D.Resource resource=D.Resource.RecordA;
        private D.Purpose purpose=D.Purpose.Care;
        private D.Action action=D.Action.Read;
        private int day=25;
        private static readonly Dictionary<string,string[]> Labels=new Dictionary<string,string[]>
        {
            {"en",new[]{"Community access simulation","Fictional identities only. Not real authentication or medical authorization.","Actor","Resource","Purpose","Action","Fixture day","Allowed in simulation","Denied","Technical reason"}},
            {"ca",new[]{"Simulació d'accés comunitari","Només identitats fictícies. No és autenticació ni autorització mèdica real.","Participant","Recurs","Finalitat","Acció","Dia simulat","Permès en la simulació","Denegat","Motiu tècnic"}},
            {"es",new[]{"Simulación de acceso comunitario","Solo identidades ficticias. No es autenticación ni autorización médica real.","Participante","Recurso","Finalidad","Acción","Día simulado","Permitido en la simulación","Denegado","Motivo técnico"}},
            {"zh",new[]{"社区访问模拟","仅虚构身份。非真实认证或医疗授权。","参与者","资源","用途","操作","模拟日期","模拟允许","拒绝","技术原因"}},
            {"hi",new[]{"समुदाय पहुँच अनुकरण","केवल काल्पनिक पहचान। वास्तविक प्रमाणीकरण या चिकित्सा प्राधिकरण नहीं।","भागीदार","संसाधन","उद्देश्य","क्रिया","काल्पनिक दिन","अनुकरण में अनुमति","अस्वीकृत","तकनीकी कारण"}},
            {"fr",new[]{"Simulation des accès communautaires","Identités fictives uniquement. Ni authentification ni autorisation médicale réelle.","Participant","Ressource","Finalité","Action","Jour simulé","Autorisé en simulation","Refusé","Motif technique"}},
            {"de",new[]{"Community-Zugriffssimulation","Nur fiktive Identitäten. Keine echte Authentifizierung oder medizinische Autorisierung.","Teilnehmer","Ressource","Zweck","Aktion","Simulierter Tag","In Simulation erlaubt","Verweigert","Technischer Grund"}},
            {"pt",new[]{"Simulação de acesso comunitário","Apenas identidades fictícias. Sem autenticação ou autorização médica real.","Participante","Recurso","Finalidade","Ação","Dia simulado","Permitido na simulação","Negado","Motivo técnico"}},
            {"ar",new[]{"محاكاة الوصول المجتمعي","هويات خيالية فقط. ليست مصادقة أو تفويضًا طبيًا حقيقيًا.","المشارك","المورد","الغرض","الإجراء","اليوم المحاكى","مسموح في المحاكاة","مرفوض","السبب التقني"}},
            {"ja",new[]{"コミュニティアクセスの模擬","架空のIDのみ。実際の認証や医療上の認可ではありません。","参加者","リソース","目的","操作","模擬日","模擬上の許可","拒否","技術的理由"}},
        };
        [MenuItem("VITQRA/Synthetic Community Policy Preview")]
        private static void Open()=>GetWindow<VitqraCommunityPreview>();
        private void OnGUI()
        {
            var l=Labels[VitqraStrings.DeviceLanguage];
            titleContent=new GUIContent(l[0]);
            EditorGUILayout.HelpBox(l[1],MessageType.Warning);
            actor=(D.Actor)EditorGUILayout.EnumPopup(l[2],actor);
            resource=(D.Resource)EditorGUILayout.EnumPopup(l[3],resource);
            purpose=(D.Purpose)EditorGUILayout.EnumPopup(l[4],purpose);
            action=(D.Action)EditorGUILayout.EnumPopup(l[5],action);
            day=EditorGUILayout.IntSlider(l[6],day,0,180);
            var decision=D.Evaluate(actor,resource,purpose,action,day);
            EditorGUILayout.HelpBox(decision.outcome==D.Outcome.AllowSimulation?l[7]:l[8],MessageType.Info);
            EditorGUILayout.LabelField(l[9],decision.reason.ToString());
            EditorGUILayout.SelectableLabel(D.Describe(actor,resource,purpose,action,day),EditorStyles.wordWrappedLabel,GUILayout.Height(60));
        }
    }
}
