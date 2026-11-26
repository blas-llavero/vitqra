using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using D=Vitqra.VitqraCommunityPolicy;

namespace Vitqra
{
    public sealed class VitqraPrevisitPreview:EditorWindow
    {
        private D.Actor actor=D.Actor.ClinicianA;
        private int record,accessDay=25,cutoff=25;
        private Vector2 scroll;
        private static readonly Dictionary<string,string[]> Labels=new Dictionary<string,string[]>
        {
            {"en",new[]{"Synthetic previsit","Fictional observations only. Not a diagnosis, medical advice or real record.","Actor","Record","Access day","Information cutoff","Access denied","Visit day","Observed / available","Missing / placeholders","Change A / B","Unsigned summary","No comparable values"}},
            {"ca",new[]{"Preparació sintètica de visita","Només observacions fictícies. No és un diagnòstic, consell mèdic ni historial real.","Participant","Historial","Dia d'accés","Tall d'informació","Accés denegat","Dia de visita","Observat / disponible","Absents / marcadors","Canvi A / B","Resum sense signar","Sense valors comparables"}},
            {"es",new[]{"Preparación sintética de visita","Solo observaciones ficticias. No es diagnóstico, consejo médico ni historial real.","Participante","Historial","Día de acceso","Corte de información","Acceso denegado","Día de visita","Observado / disponible","Ausentes / marcadores","Cambio A / B","Resumen sin firmar","Sin valores comparables"}},
            {"zh",new[]{"合成就诊准备","仅虚构观测。非诊断、医疗建议或真实病历。","参与者","记录","访问日","信息截止","拒绝访问","就诊日","观测 / 可用","缺失 / 占位","变化 A / B","未签署摘要","无可比较值"}},
            {"hi",new[]{"कृत्रिम पूर्व-भेंट तैयारी","केवल काल्पनिक अवलोकन। निदान, चिकित्सा सलाह या वास्तविक रिकॉर्ड नहीं।","भागीदार","रिकॉर्ड","पहुँच दिन","सूचना सीमा","पहुँच अस्वीकृत","भेंट दिन","अवलोकन / उपलब्ध","अनुपलब्ध / संकेतक","बदलाव A / B","अहस्ताक्षरित सारांश","तुलनीय मान नहीं"}},
            {"fr",new[]{"Préparation synthétique de consultation","Observations fictives uniquement. Ni diagnostic, ni conseil médical, ni dossier réel.","Participant","Dossier","Jour d'accès","Limite des informations","Accès refusé","Jour de consultation","Observé / disponible","Manquants / marqueurs","Variation A / B","Résumé non signé","Pas de valeurs comparables"}},
            {"de",new[]{"Synthetische Besuchsvorbereitung","Nur fiktive Beobachtungen. Keine Diagnose, medizinische Beratung oder echte Akte.","Teilnehmer","Akte","Zugriffstag","Informationsstichtag","Zugriff verweigert","Besuchstag","Beobachtet / verfügbar","Fehlend / Platzhalter","Änderung A / B","Unsignierte Zusammenfassung","Keine vergleichbaren Werte"}},
            {"pt",new[]{"Preparação sintética da consulta","Apenas observações fictícias. Sem diagnóstico, aconselhamento médico ou registo real.","Participante","Registo","Dia de acesso","Limite de informação","Acesso negado","Dia da consulta","Observado / disponível","Ausentes / marcadores","Alteração A / B","Resumo não assinado","Sem valores comparáveis"}},
            {"ar",new[]{"تحضير زيارة اصطناعية","ملاحظات خيالية فقط. ليست تشخيصًا أو نصيحة طبية أو سجلًا حقيقيًا.","المشارك","السجل","يوم الوصول","حد المعلومات","رفض الوصول","يوم الزيارة","الملاحظة / التوفر","مفقود / عناصر نائبة","التغير A / B","ملخص غير موقع","لا قيم قابلة للمقارنة"}},
            {"ja",new[]{"合成受診準備","架空の観測のみ。診断、医療助言、実際の記録ではありません。","参加者","記録","アクセス日","情報締切","アクセス拒否","受診日","観測 / 利用可能","欠損 / プレースホルダー","変化 A / B","未署名の要約","比較可能な値なし"}},
        };
        [MenuItem("VITQRA/Synthetic Previsit Preview")]
        private static void Open()=>GetWindow<VitqraPrevisitPreview>();
        private static string V(int? x,string missing)=>x.HasValue?x.Value.ToString(CultureInfo.InvariantCulture):missing;
        private void OnGUI()
        {
            var l=Labels[VitqraStrings.DeviceLanguage];titleContent=new GUIContent(l[0]);
            EditorGUILayout.HelpBox(l[1],MessageType.Warning);
            actor=(D.Actor)EditorGUILayout.EnumPopup(l[2],actor);
            record=EditorGUILayout.Popup(l[3],record,new[]{"RecordA","RecordB"});
            accessDay=EditorGUILayout.IntSlider(l[4],accessDay,0,180);
            cutoff=EditorGUILayout.IntSlider(l[5],cutoff,0,29);
            // Do not cache a previous authorized summary when actor/time changes.
            var result=VitqraPrevisit.Open(actor,record==0?D.Resource.RecordA:D.Resource.RecordB,accessDay,cutoff);
            var s=result.summary;
            if(s==null){EditorGUILayout.HelpBox(l[6]+" ("+result.reason+")",MessageType.Info);return;}
            scroll=EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField(l[11],EditorStyles.boldLabel);
            EditorGUILayout.LabelField(l[7],VitqraLongitudinal.Date(s.visitDay));
            EditorGUILayout.LabelField(l[9],s.knownMissing+" / "+s.placeholders);
            EditorGUILayout.LabelField(l[10],V(s.changeA,l[12])+" / "+V(s.changeB,l[12]));
            Rect rect=GUILayoutUtility.GetRect(100,120,GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect,new Color(.08f,.12f,.16f));
            foreach(var o in s.observations)
            {
                if(!o.value.HasValue||(o.kind!=VitqraPrevisit.Kind.NumericA&&o.kind!=VitqraPrevisit.Kind.NumericB))continue;
                float x=rect.x+5+(rect.width-10)*o.observed/30f;
                float y=rect.yMax-5-(rect.height-10)*Mathf.Clamp01(o.value.Value/100f);
                EditorGUI.DrawRect(new Rect(x-2,y-2,5,5),o.kind==VitqraPrevisit.Kind.NumericA?Color.cyan:new Color(.8f,.6f,1));
            }
            EditorGUILayout.LabelField("A / B",EditorStyles.boldLabel);
            foreach(var o in s.observations)
            {
                EditorGUILayout.LabelField(o.kind+" | "+l[8]+": "+o.observed+" / "+o.available);
                EditorGUILayout.LabelField(o.source+" | "+o.status+(o.value.HasValue?" | "+V(o.value,""):""));
            }
            EditorGUILayout.SelectableLabel(s.digest,EditorStyles.textField,GUILayout.Height(36));
            EditorGUILayout.EndScrollView();
        }
    }
}
