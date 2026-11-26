using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace Vitqra
{
    // Editor preview only, not part of a clinical or patient-facing runtime.
    public sealed class VitqraLongitudinalPreview : EditorWindow
    {
        private int subject,day=60;
        private Vector2 scroll;
        private VitqraRetrospective.Result experiment;
        private static readonly Dictionary<string,string[]> Labels=new Dictionary<string,string[]>
        {
            {"en",new[]{"Synthetic timeline","Fictional data only. No clinical prediction.","Subject","Information cutoff (days)","Observed / available","Value","Missing","Run synthetic retrospective experiment","Development / holdout","Excluded: follow-up / measurements","Threshold","Balanced accuracy"}},
            {"ca",new[]{"Cronologia sintètica","Només dades fictícies. Cap predicció clínica.","Subjecte","Tall d'informació (dies)","Observat / disponible","Valor","Absent","Executa l'experiment retrospectiu sintètic","Desenvolupament / avaluació","Exclosos: seguiment / mesures","Llindar","Exactitud equilibrada"}},
            {"es",new[]{"Cronología sintética","Solo datos ficticios. Sin predicción clínica.","Sujeto","Corte de información (días)","Observado / disponible","Valor","Ausente","Ejecutar experimento retrospectivo sintético","Desarrollo / evaluación","Excluidos: seguimiento / medidas","Umbral","Exactitud equilibrada"}},
            {"zh",new[]{"合成时间线","仅虚构数据。无临床预测。","对象","信息截止（天）","观察 / 可用","值","缺失","运行合成回顾性实验","开发 / 评估","排除：随访 / 测量","阈值","平衡准确率"}},
            {"hi",new[]{"कृत्रिम समयरेखा","केवल काल्पनिक डेटा। कोई नैदानिक पूर्वानुमान नहीं।","विषय","सूचना सीमा (दिन)","अवलोकन / उपलब्ध","मान","अनुपलब्ध","कृत्रिम पूर्वव्यापी प्रयोग चलाएँ","विकास / मूल्यांकन","बहिष्कृत: अनुवर्ती / माप","सीमा","संतुलित सटीकता"}},
            {"fr",new[]{"Chronologie synthétique","Données fictives uniquement. Aucune prédiction clinique.","Sujet","Date limite (jours)","Observé / disponible","Valeur","Manquante","Lancer l'expérience rétrospective synthétique","Développement / évaluation","Exclus : suivi / mesures","Seuil","Exactitude équilibrée"}},
            {"de",new[]{"Synthetischer Zeitverlauf","Nur fiktive Daten. Keine klinische Prognose.","Person","Informationsstichtag (Tage)","Beobachtet / verfügbar","Wert","Fehlend","Synthetisches retrospektives Experiment starten","Entwicklung / Auswertung","Ausgeschlossen: Nachbeobachtung / Messungen","Schwelle","Ausgewogene Genauigkeit"}},
            {"pt",new[]{"Cronologia sintética","Apenas dados fictícios. Sem previsão clínica.","Sujeito","Limite de informação (dias)","Observado / disponível","Valor","Ausente","Executar experiência retrospetiva sintética","Desenvolvimento / avaliação","Excluídos: seguimento / medições","Limiar","Exatidão equilibrada"}},
            {"ar",new[]{"خط زمني اصطناعي","بيانات خيالية فقط. لا تنبؤ سريري.","الموضوع","حد المعلومات (أيام)","الملاحظة / التوفر","القيمة","مفقودة","تشغيل تجربة استعادية اصطناعية","التطوير / التقييم","مستبعدون: المتابعة / القياسات","العتبة","الدقة المتوازنة"}},
            {"ja",new[]{"合成タイムライン","架空データのみ。臨床予測なし。","対象","情報締切（日）","観測 / 利用可能","値","欠損","合成後ろ向き実験を実行","開発 / 評価","除外：追跡 / 測定","閾値","均衡精度"}}
        };
        [MenuItem("VITQRA/Synthetic Longitudinal Preview")]
        private static void Open()=>GetWindow<VitqraLongitudinalPreview>();
        private void OnGUI()
        {
            var l=Labels[VitqraStrings.DeviceLanguage];
            titleContent=new GUIContent(l[0]);
            EditorGUILayout.HelpBox(l[1],MessageType.Info);
            subject=EditorGUILayout.Popup(l[2],subject,new[]{"SYN-001","SYN-002","SYN-003"});
            day=EditorGUILayout.IntSlider(l[3],day,0,180);
            var snap=VitqraLongitudinal.At(subject,day);
            if(snap==null)return;
            scroll=EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField(VitqraLongitudinal.Date(day));
            Rect rect=GUILayoutUtility.GetRect(100,120,GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect,new Color(.08f,.12f,.16f));
            foreach(var row in snap.observations)
            {
                if(!row.value.HasValue)continue;
                float x=rect.x+5+(rect.width-10)*row.observedDay/180f;
                float y=rect.yMax-5-(rect.height-10)*Mathf.Clamp01(row.value.Value/100f);
                EditorGUI.DrawRect(new Rect(x-2,y-2,5,5),row.feature==VitqraLongitudinal.Feature.A?Color.cyan:new Color(.8f,.6f,1));
            }
            EditorGUILayout.LabelField("A — cyan | B — violet");
            foreach(var row in snap.observations)
                EditorGUILayout.LabelField(row.feature+" | "+l[4]+": "+row.observedDay+" / "+row.availableDay,l[5]+": "+(row.value.HasValue?row.value.Value.ToString(CultureInfo.InvariantCulture):l[6]));
            EditorGUILayout.SelectableLabel(snap.digest,EditorStyles.textField,GUILayout.Height(36));
            if(GUILayout.Button(l[7]))experiment=VitqraRetrospective.Run();
            if(experiment!=null)
            {
                EditorGUILayout.LabelField(l[8],experiment.train.Total+" / "+experiment.test.Total);
                EditorGUILayout.LabelField(l[9],(experiment.trainCensored+experiment.testCensored)+" / "+(experiment.trainMissing+experiment.testMissing));
                EditorGUILayout.LabelField(l[10],experiment.threshold.ToString(CultureInfo.InvariantCulture));
                var m=experiment.test;
                EditorGUILayout.LabelField("TP / FP / TN / FN",m.TP+" / "+m.FP+" / "+m.TN+" / "+m.FN);
                EditorGUILayout.LabelField(l[11],m.BalancedAccuracy.HasValue?m.BalancedAccuracy.Value.ToString("P2",CultureInfo.InvariantCulture):"—");
                EditorGUILayout.SelectableLabel(experiment.cohortDigest,EditorStyles.textField,GUILayout.Height(36));
            }
            EditorGUILayout.EndScrollView();
        }
    }
}
