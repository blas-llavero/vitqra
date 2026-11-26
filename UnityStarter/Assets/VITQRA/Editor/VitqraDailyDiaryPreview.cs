using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using D=Vitqra.VitqraCommunityPolicy;
using M=Vitqra.VitqraDailyDiary;

namespace Vitqra
{
    public sealed class VitqraDailyDiaryPreview:EditorWindow
    {
        private M diary;
        private int protocol,requestedDay=1,hour=18,simHour=18;
        private bool extra,reminderShown;
        private string result="";
        private Vector2 scroll;
        private const D.Actor Owner=D.Actor.PatientA;
        private static readonly Dictionary<string,string[]> Labels=new Dictionary<string,string[]>
        {
            {"en",new[]{"Fictional daily diary","DEMO ONLY. Do not enter real health information.","Protocol","Daily (demo)","Alternate days (demo)","New fictional session / clear","Day","Scheduled response","Extra response","No symptoms today","I feel better","I feel the same","I feel worse","I do not know","I could not take the measurement","Voluntary reminders","Reminder hour","Simulated current hour","Check reminder","Time for your optional check-in.","Authorize simulated device","Import simulated measurement","Expected / answered","State known / unknown / unable","Unanswered / extra / device","History","Result","Not a clinical or research approval","Day not scheduled"}},
            {"ca",new[]{"Diari diari fictici","NOMÉS DEMOSTRACIÓ. No introdueixis informació de salut real.","Protocol","Diari (demostració)","Dies alterns (demostració)","Nova sessió fictícia / esborra","Dia","Resposta programada","Resposta addicional","Avui no tinc símptomes","Em trobo millor","Em trobo igual","Em trobo pitjor","No ho sé","No he pogut fer la mesura","Recordatoris voluntaris","Hora del recordatori","Hora actual simulada","Comprova el recordatori","És hora del registre voluntari.","Autoritza dispositiu simulat","Importa mesura simulada","Previstos / respostos","Estat conegut / desconegut / no mesurat","Sense resposta / extra / dispositiu","Historial","Resultat","No és una aprovació clínica ni científica","Dia no programat"}},
            {"es",new[]{"Diario ficticio","SOLO DEMOSTRACIÓN. No introduzcas información de salud real.","Protocolo","Diario (demostración)","Días alternos (demostración)","Nueva sesión ficticia / borrar","Día","Respuesta programada","Respuesta adicional","Hoy no tengo síntomas","Me encuentro mejor","Me encuentro igual","Me encuentro peor","No lo sé","No he podido hacer la medición","Recordatorios voluntarios","Hora del recordatorio","Hora actual simulada","Comprobar recordatorio","Es hora del registro voluntario.","Autorizar dispositivo simulado","Importar medida simulada","Previstos / respondidos","Estado conocido / desconocido / no medido","Sin respuesta / extra / dispositivo","Historial","Resultado","No es una aprobación clínica ni científica","Día no programado"}},
            {"zh",new[]{"虚构日记","仅演示。请勿输入真实健康信息。","方案","每日（演示）","隔日（演示）","新虚构会话 / 清除","日","计划回答","额外回答","今天没有症状","感觉好些","感觉一样","感觉更差","不知道","未能测量","自愿提醒","提醒小时","模拟当前小时","检查提醒","可自愿填写记录。","授权模拟设备","导入模拟测量","预期 / 已答","已知 / 未知 / 未测","未答 / 额外 / 设备","历史","结果","非临床或研究批准","非计划日"}},
            {"hi",new[]{"काल्पनिक दैनिक डायरी","केवल प्रदर्शन। वास्तविक स्वास्थ्य जानकारी न डालें।","प्रोटोकॉल","दैनिक (डेमो)","एक दिन छोड़कर (डेमो)","नया काल्पनिक सत्र / मिटाएँ","दिन","निर्धारित उत्तर","अतिरिक्त उत्तर","आज कोई लक्षण नहीं","मैं बेहतर महसूस करता हूँ","मैं वैसा ही महसूस करता हूँ","मैं बदतर महसूस करता हूँ","मुझे नहीं पता","मैं माप नहीं ले सका","स्वैच्छिक अनुस्मारक","अनुस्मारक का घंटा","अनुकरण का वर्तमान घंटा","अनुस्मारक जाँचें","स्वैच्छिक प्रविष्टि का समय है।","काल्पनिक उपकरण अधिकृत करें","काल्पनिक माप आयात करें","अपेक्षित / उत्तरित","ज्ञात / अज्ञात / माप नहीं","अनुत्तरित / अतिरिक्त / उपकरण","इतिहास","परिणाम","नैदानिक या शोध स्वीकृति नहीं","निर्धारित दिन नहीं"}},
            {"fr",new[]{"Journal quotidien fictif","DÉMONSTRATION. Ne saisissez aucune donnée de santé réelle.","Protocole","Quotidien (démo)","Jours alternés (démo)","Nouvelle session fictive / effacer","Jour","Réponse planifiée","Réponse supplémentaire","Aucun symptôme aujourd'hui","Je me sens mieux","Je me sens pareil","Je me sens moins bien","Je ne sais pas","Je n'ai pas pu effectuer la mesure","Rappels volontaires","Heure du rappel","Heure actuelle simulée","Vérifier le rappel","C'est le moment du suivi volontaire.","Autoriser le dispositif simulé","Importer une mesure simulée","Prévus / répondus","État connu / inconnu / non mesuré","Sans réponse / extra / dispositif","Historique","Résultat","Ni approbation clinique ni scientifique","Jour non planifié"}},
            {"de",new[]{"Fiktives Tagebuch","NUR DEMO. Keine echten Gesundheitsdaten eingeben.","Protokoll","Täglich (Demo)","Jeden zweiten Tag (Demo)","Neue fiktive Sitzung / löschen","Tag","Geplante Antwort","Zusätzliche Antwort","Heute keine Symptome","Mir geht es besser","Mir geht es gleich","Mir geht es schlechter","Ich weiß es nicht","Ich konnte nicht messen","Freiwillige Erinnerungen","Erinnerungsstunde","Simulierte aktuelle Stunde","Erinnerung prüfen","Zeit für den freiwilligen Eintrag.","Simuliertes Gerät autorisieren","Simulierte Messung importieren","Erwartet / beantwortet","Bekannt / unbekannt / nicht gemessen","Unbeantwortet / extra / Gerät","Verlauf","Ergebnis","Keine klinische oder wissenschaftliche Freigabe","Nicht geplanter Tag"}},
            {"pt",new[]{"Diário fictício","APENAS DEMONSTRAÇÃO. Não introduza dados de saúde reais.","Protocolo","Diário (demo)","Dias alternados (demo)","Nova sessão fictícia / limpar","Dia","Resposta programada","Resposta adicional","Hoje não tenho sintomas","Sinto-me melhor","Sinto-me igual","Sinto-me pior","Não sei","Não consegui fazer a medição","Lembretes voluntários","Hora do lembrete","Hora atual simulada","Verificar lembrete","É hora do registo voluntário.","Autorizar dispositivo simulado","Importar medição simulada","Previstos / respondidos","Conhecido / desconhecido / não medido","Sem resposta / extra / dispositivo","Histórico","Resultado","Não é aprovação clínica nem científica","Dia não programado"}},
            {"ar",new[]{"يوميات خيالية","عرض تجريبي فقط. لا تدخل معلومات صحية حقيقية.","البروتوكول","يومي (تجريبي)","يوم بعد يوم (تجريبي)","جلسة خيالية جديدة / مسح","اليوم","إجابة مجدولة","إجابة إضافية","لا أعراض لدي اليوم","أشعر بتحسن","أشعر كما كنت","أشعر بأنني أسوأ","لا أعرف","لم أتمكن من القياس","تذكيرات طوعية","ساعة التذكير","الساعة الحالية المحاكية","تحقق من التذكير","حان وقت التسجيل الطوعي.","تفويض جهاز محاكى","استيراد قياس محاكى","متوقع / مجاب","معروف / مجهول / غير مقاس","بلا إجابة / إضافي / جهاز","السجل","النتيجة","ليس اعتمادًا سريريًا أو علميًا","يوم غير مجدول"}},
            {"ja",new[]{"架空の日誌","デモ専用。実際の健康情報を入力しないでください。","プロトコル","毎日（デモ）","隔日（デモ）","新しい架空セッション / 消去","日","予定回答","追加回答","今日は症状がない","良くなった","変わらない","悪くなった","わからない","測定できなかった","任意のリマインダー","通知時刻","模擬現在時刻","リマインダーを確認","任意記録の時間です。","模擬機器を許可","模擬測定を取り込む","予定 / 回答済み","既知 / 不明 / 未測定","未回答 / 追加 / 機器","履歴","結果","臨床・研究承認ではない","予定外の日"}},
        };
        [MenuItem("VITQRA/Synthetic Daily Diary Preview")]
        private static void Open()=>GetWindow<VitqraDailyDiaryPreview>();
        private void OnGUI()
        {
            var l=Labels[VitqraStrings.DeviceLanguage];titleContent=new GUIContent(l[0]);
            EditorGUILayout.HelpBox(l[1],MessageType.Warning);
            if(diary==null)diary=new M(D.Resource.RecordA,M.Protocol.DailyDemo);
            scroll=EditorGUILayout.BeginScrollView(scroll);
            protocol=EditorGUILayout.Popup(l[2],protocol,new[]{l[3],l[4]});
            if(GUILayout.Button(l[5]))
            {
                diary=new M(D.Resource.RecordA,(M.Protocol)protocol);requestedDay=1;hour=18;simHour=18;
                extra=false;result="";reminderShown=false;
            }
            EditorGUILayout.LabelField(l[2],l[3+(int)diary.protocol]);
            requestedDay=EditorGUILayout.IntSlider(l[6],requestedDay,diary.CurrentDay,30);
            if(requestedDay>diary.CurrentDay){diary.Advance(Owner,requestedDay);reminderShown=false;result="";}
            extra=EditorGUILayout.Toggle(l[8],extra);
            EditorGUILayout.LabelField(extra?l[8]:l[7],EditorStyles.boldLabel);
            if(!extra&&!diary.Scheduled(diary.CurrentDay))EditorGUILayout.LabelField(l[28]);
            using(new EditorGUI.DisabledScope(!extra&&!diary.Scheduled(diary.CurrentDay)))
                for(int i=0;i<6;i++)
                    if(GUILayout.Button(l[9+i],GUILayout.MinHeight(32)))
                    {result=diary.Record(Owner,(M.State)i,extra).ToString();if(!extra)reminderShown=false;}
            bool reminder=EditorGUILayout.Toggle(l[15],diary.ReminderOptIn);
            hour=EditorGUILayout.IntSlider(l[16],hour,0,23);
            if(reminder!=diary.ReminderOptIn||hour*60!=diary.ReminderMinute)diary.SetReminder(Owner,reminder,hour*60);
            if(!reminder)reminderShown=false;
            simHour=EditorGUILayout.IntSlider(l[17],simHour,0,23);
            if(GUILayout.Button(l[18]))reminderShown=diary.PollReminder(Owner,simHour*60);
            if(reminderShown)EditorGUILayout.HelpBox(l[19],MessageType.Info);
            bool device=EditorGUILayout.Toggle(l[20],diary.DeviceOptIn);
            if(device!=diary.DeviceOptIn)diary.SetDeviceAuthorization(Owner,device);
            using(new EditorGUI.DisabledScope(!device))
                if(GUILayout.Button(l[21]))result=diary.ImportSimulatedDevice(Owner).ToString();
            var c=diary.Inspect(Owner,diary.CurrentDay);
            EditorGUILayout.LabelField(l[22],c.expected+" / "+c.responded);
            EditorGUILayout.LabelField(l[23],c.usableState+" / "+c.unknown+" / "+c.unable);
            EditorGUILayout.LabelField(l[24],c.unanswered+" / "+c.extras+" / "+c.device);
            for(int i=0;i<6;i++)EditorGUILayout.LabelField(l[9+i],c.distribution[i].ToString());
            EditorGUILayout.LabelField(l[25],EditorStyles.boldLabel);
            foreach(var e in diary.Snapshot(Owner,diary.CurrentDay))
                EditorGUILayout.LabelField(l[6]+" "+e.day+" | "+e.source,e.state.HasValue?l[9+(int)e.state.Value]:e.unitlessValue.ToString());
            EditorGUILayout.LabelField(l[26],result);
            EditorGUILayout.HelpBox(l[27],MessageType.Info);
            EditorGUILayout.EndScrollView();
        }
    }
}
