using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using D=Vitqra.VitqraCommunityPolicy;
using M=Vitqra.VitqraValidationCenter;
namespace Vitqra
{
    public sealed class VitqraValidationCenterPreview:EditorWindow
    {
        private M center;
        private M.Reviewer reviewer;
        private M.Verdict verdict=M.Verdict.Revise;
        private M.Rationale rationale=M.Rationale.DataIncomplete;
        private Vector2 scroll;
        private string result="";
        private static readonly Dictionary<string,string[]> Labels=new Dictionary<string,string[]>
        {
            {"en",new[]{"Synthetic validation center","DEMO ONLY. No actual reviewer, certificate or scientific/clinical approval.","Reviewer fixture","Access day","Access denied","Revision / quality","Review status","Verdict","Rationale","Submit unsigned draft","Simulate corrected revision","History","Result","Actual release blocked"}},
            {"ca",new[]{"Centre de validació sintètic","NOMÉS DEMOSTRACIÓ. Sense revisor, certificat ni aprovació científica o clínica real.","Revisor fictici","Dia d'accés","Accés denegat","Revisió / qualitat","Estat de revisió","Dictamen","Motiu","Envia esborrany sense signar","Simula revisió corregida","Historial","Resultat","Ús real bloquejat"}},
            {"es",new[]{"Centro de validación sintético","SOLO DEMOSTRACIÓN. Sin revisor, certificado ni aprobación científica o clínica real.","Revisor ficticio","Día de acceso","Acceso denegado","Revisión / calidad","Estado de revisión","Dictamen","Motivo","Enviar borrador sin firmar","Simular revisión corregida","Historial","Resultado","Uso real bloqueado"}},
            {"zh",new[]{"合成验证中心","仅演示。无真实审查员、证书或科学/临床批准。","模拟审查员","访问日","拒绝访问","版本 / 质量","审查状态","意见","理由","提交未签名草稿","模拟修订","历史","结果","禁止实际发布"}},
            {"hi",new[]{"कृत्रिम सत्यापन केंद्र","केवल प्रदर्शन। वास्तविक समीक्षक, प्रमाणपत्र या वैज्ञानिक/नैदानिक स्वीकृति नहीं।","काल्पनिक समीक्षक","पहुँच दिन","पहुँच अस्वीकृत","संशोधन / गुणवत्ता","समीक्षा स्थिति","निर्णय","कारण","अहस्ताक्षरित मसौदा भेजें","सुधारा संस्करण अनुकरण","इतिहास","परिणाम","वास्तविक उपयोग अवरुद्ध"}},
            {"fr",new[]{"Centre de validation synthétique","DÉMONSTRATION. Aucun évaluateur réel, certificat ou agrément scientifique/clinique.","Évaluateur fictif","Jour d’accès","Accès refusé","Révision / qualité","État de revue","Avis","Motif","Soumettre un brouillon non signé","Simuler une révision corrigée","Historique","Résultat","Usage réel bloqué"}},
            {"de",new[]{"Synthetisches Validierungszentrum","NUR DEMO. Keine echten Gutachter, Zertifikate oder wissenschaftliche/klinische Freigabe.","Fiktiver Gutachter","Zugriffstag","Zugriff verweigert","Revision / Qualität","Prüfstatus","Urteil","Grund","Unsignierten Entwurf einreichen","Korrigierte Revision simulieren","Verlauf","Ergebnis","Reale Freigabe gesperrt"}},
            {"pt",new[]{"Centro de validação sintético","APENAS DEMO. Sem revisor real, certificado ou aprovação científica/clínica.","Revisor fictício","Dia de acesso","Acesso negado","Revisão / qualidade","Estado da revisão","Parecer","Motivo","Enviar rascunho não assinado","Simular revisão corrigida","Histórico","Resultado","Uso real bloqueado"}},
            {"ar",new[]{"مركز تحقق اصطناعي","عرض فقط. لا مراجع حقيقي أو شهادة أو اعتماد علمي أو سريري.","مراجع خيالي","يوم الوصول","رفض الوصول","المراجعة / الجودة","حالة المراجعة","القرار","السبب","إرسال مسودة غير موقعة","محاكاة نسخة مصححة","السجل","النتيجة","الاستخدام الحقيقي محظور"}},
            {"ja",new[]{"合成検証センター","デモ専用。実際の査読者、証明書、科学的・臨床的承認なし。","架空の査読者","アクセス日","アクセス拒否","改訂 / 品質","審査状態","判定","理由","未署名の草稿を提出","修正版を模擬","履歴","結果","実利用は不可"}},
        };
        [MenuItem("VITQRA/Synthetic Validation Center Preview")]
        private static void Open()=>GetWindow<VitqraValidationCenterPreview>();
        private void OnGUI()
        {
            var l=Labels[VitqraStrings.DeviceLanguage];
            titleContent=new GUIContent(l[0]);
            if(center==null)center=new M();
            EditorGUILayout.HelpBox(l[1],MessageType.Warning);
            var actor=D.Actor.Reviewer;
            reviewer=(M.Reviewer)EditorGUILayout.EnumPopup(l[2],reviewer);
            center.Advance(EditorGUILayout.IntSlider(l[3],center.Day,center.Day,180));
            var r=center.Open(actor,reviewer);
            if(r==null){result="";EditorGUILayout.HelpBox(l[4],MessageType.Info);return;}
            scroll=EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField(l[5],r.number+" / "+r.quality.gate);
            EditorGUILayout.LabelField("Observed / expected",r.quality.observed+" / "+r.quality.expected);
            EditorGUILayout.SelectableLabel(r.digest,EditorStyles.textField,GUILayout.Height(36));
            EditorGUILayout.LabelField(l[6],center.Inspect(actor,reviewer).ToString());
            verdict=(M.Verdict)EditorGUILayout.EnumPopup(l[7],verdict);
            rationale=(M.Rationale)EditorGUILayout.EnumPopup(l[8],rationale);
            if(GUILayout.Button(l[9]))result=center.Submit(actor,reviewer,r.digest,verdict,rationale).ToString();
            if(GUILayout.Button(l[10]))result=center.SimulateCorrectedRevision(actor,reviewer)?"Saved":"Denied";
            EditorGUILayout.LabelField(l[12],result);
            EditorGUILayout.LabelField(l[11],EditorStyles.boldLabel);
            foreach(var v in center.History(actor,reviewer))
                EditorGUILayout.LabelField("#"+v.revision+" | "+v.reviewer,v.verdict+" | "+v.rationale);
            EditorGUILayout.HelpBox(l[13],MessageType.Warning);
            EditorGUILayout.EndScrollView();
        }
    }
}
