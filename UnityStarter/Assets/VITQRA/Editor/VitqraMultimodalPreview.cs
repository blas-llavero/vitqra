using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace Vitqra
{
    public sealed class VitqraMultimodalPreview:EditorWindow
    {
        private VitqraMultimodal.Result result;
        private Vector2 scroll;
        private static readonly Dictionary<string,string[]> Labels=new Dictionary<string,string[]>
        {
            {"en",new[]{"Synthetic multimodal research","Fictional proxies only. No patient data or clinical predictions.","Run experiment","Development / holdout eligible","Excluded: unknown outcome / incomplete features","Temporal","Image proxy","Symptom proxy","Genetic proxy","Fusion","Balanced accuracy","Sensitivity / specificity (95% Wilson intervals)","Available modalities (development / holdout)","Always-negative baseline"}},
            {"ca",new[]{"Recerca multimodal sintètica","Només variables fictícies. Cap dada de pacient ni predicció clínica.","Executa l'experiment","Desenvolupament / avaluació elegibles","Exclosos: resultat desconegut / variables incompletes","Temporal","Variable d'imatge","Variable de símptomes","Variable genètica","Fusió","Exactitud equilibrada","Sensibilitat / especificitat (intervals Wilson del 95%)","Modalitats disponibles (desenvolupament / avaluació)","Referència sempre negativa"}},
            {"es",new[]{"Investigación multimodal sintética","Solo variables ficticias. Sin datos de pacientes ni predicciones clínicas.","Ejecutar experimento","Desarrollo / evaluación elegibles","Excluidos: resultado desconocido / variables incompletas","Temporal","Variable de imagen","Variable de síntomas","Variable genética","Fusión","Exactitud equilibrada","Sensibilidad / especificidad (intervalos Wilson del 95%)","Modalidades disponibles (desarrollo / evaluación)","Referencia siempre negativa"}},
            {"zh",new[]{"合成多模态研究","仅虚构特征，无患者数据或临床预测。","运行实验","开发 / 评估可用人数","排除：结局未知 / 特征不全","时间","影像代理","症状代理","遗传代理","融合","平衡准确率","灵敏度 / 特异度（95% Wilson 区间）","可用模态（开发 / 评估）","始终阴性基线"}},
            {"hi",new[]{"कृत्रिम बहुविध शोध","केवल काल्पनिक चर। रोगी डेटा या नैदानिक पूर्वानुमान नहीं।","प्रयोग चलाएँ","विकास / मूल्यांकन योग्य","बहिष्कृत: अज्ञात परिणाम / अधूरे चर","कालिक","छवि प्रतिनिधि","लक्षण प्रतिनिधि","आनुवंशिक प्रतिनिधि","संयोजन","संतुलित सटीकता","संवेदनशीलता / विशिष्टता (95% Wilson अंतराल)","उपलब्ध विधाएँ (विकास / मूल्यांकन)","सदैव नकारात्मक आधार"}},
            {"fr",new[]{"Recherche multimodale synthétique","Variables fictives uniquement. Ni données de patients ni prédictions cliniques.","Lancer l’expérience","Développement / évaluation admissibles","Exclus : issue inconnue / variables incomplètes","Temporel","Proxy image","Proxy symptômes","Proxy génétique","Fusion","Exactitude équilibrée","Sensibilité / spécificité (intervalles Wilson à 95 %)","Modalités disponibles (développement / évaluation)","Référence toujours négative"}},
            {"de",new[]{"Synthetische multimodale Forschung","Nur fiktive Merkmale. Keine Patientendaten oder klinischen Prognosen.","Experiment starten","Entwicklung / Auswertung geeignet","Ausgeschlossen: unbekanntes Ergebnis / fehlende Merkmale","Zeitlich","Bild-Proxy","Symptom-Proxy","Genetik-Proxy","Fusion","Ausgewogene Genauigkeit","Sensitivität / Spezifität (95 % Wilson-Intervalle)","Verfügbare Modalitäten (Entwicklung / Auswertung)","Immer negative Referenz"}},
            {"pt",new[]{"Investigação multimodal sintética","Apenas variáveis fictícias. Sem dados de pacientes ou previsões clínicas.","Executar experiência","Desenvolvimento / avaliação elegíveis","Excluídos: desfecho desconhecido / variáveis incompletas","Temporal","Variável de imagem","Variável de sintomas","Variável genética","Fusão","Exatidão equilibrada","Sensibilidade / especificidade (intervalos Wilson de 95%)","Modalidades disponíveis (desenvolvimento / avaliação)","Referência sempre negativa"}},
            {"ar",new[]{"بحث اصطناعي متعدد الوسائط","متغيرات خيالية فقط. لا بيانات مرضى أو تنبؤات سريرية.","تشغيل التجربة","المؤهلون للتطوير / التقييم","مستبعدون: نتيجة مجهولة / خصائص ناقصة","زمني","بديل الصورة","بديل الأعراض","بديل وراثي","دمج","الدقة المتوازنة","الحساسية / النوعية (فواصل Wilson بنسبة 95%)","الوسائط المتاحة (التطوير / التقييم)","مرجع سلبي دائمًا"}},
            {"ja",new[]{"合成マルチモーダル研究","架空の特徴のみ。患者データや臨床予測なし。","実験を実行","開発 / 評価の適格数","除外：転帰不明 / 特徴不足","時系列","画像代理","症状代理","遺伝代理","融合","均衡精度","感度 / 特異度（95% Wilson 区間）","利用可能なモダリティ（開発 / 評価）","常に陰性の基準"}},
        };
        [MenuItem("VITQRA/Synthetic Multimodal Preview")]
        private static void Open()=>GetWindow<VitqraMultimodalPreview>();
        private static string P(double? v)=>v.HasValue?v.Value.ToString("P2",CultureInfo.InvariantCulture):"—";
        private static string CI(int k,int n)
        {
            var i=VitqraMultimodal.Metrics.Wilson(k,n);
            return i==null?"—":P(i[0])+" – "+P(i[1]);
        }
        private void OnGUI()
        {
            var l=Labels[VitqraStrings.DeviceLanguage];
            titleContent=new GUIContent(l[0]);
            EditorGUILayout.HelpBox(l[1],MessageType.Info);
            if(GUILayout.Button(l[2]))result=VitqraMultimodal.Run();
            if(result==null)return;
            scroll=EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField(l[3],result.models[0].development.Total+" / "+result.models[0].holdout.Total);
            EditorGUILayout.LabelField(l[4],(result.developmentUnknown+result.holdoutUnknown)+" / "+(result.developmentMissing+result.holdoutMissing));
            EditorGUILayout.LabelField(l[12],EditorStyles.boldLabel);
            for(int i=0;i<4;i++)EditorGUILayout.LabelField(l[5+i],result.developmentAvailable[i]+" / "+result.holdoutAvailable[i]);
            foreach(var model in result.models)
            {
                var m=model.holdout;
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(l[5+(int)model.modality],EditorStyles.boldLabel);
                EditorGUILayout.LabelField(l[10],P(m.BalancedAccuracy));
                var rect=GUILayoutUtility.GetRect(80,12,GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(rect,new Color(.12f,.18f,.22f));
                if(m.BalancedAccuracy.HasValue)EditorGUI.DrawRect(new Rect(rect.x,rect.y,rect.width*(float)m.BalancedAccuracy.Value,rect.height),Color.cyan);
                EditorGUILayout.LabelField("TP / FP / TN / FN",m.TP+" / "+m.FP+" / "+m.TN+" / "+m.FN);
                EditorGUILayout.LabelField(l[11],EditorStyles.wordWrappedLabel);
                EditorGUILayout.LabelField(P(m.Sensitivity)+" ["+CI(m.TP,m.TP+m.FN)+"]");
                EditorGUILayout.LabelField(P(m.Specificity)+" ["+CI(m.TN,m.TN+m.FP)+"]");
            }
            EditorGUILayout.LabelField(l[13],P(result.baseline.BalancedAccuracy));
            EditorGUILayout.SelectableLabel(result.cohortDigest,EditorStyles.textField,GUILayout.Height(36));
            EditorGUILayout.EndScrollView();
        }
    }
}
