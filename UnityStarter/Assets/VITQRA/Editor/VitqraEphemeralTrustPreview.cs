using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using M=Vitqra.VitqraEphemeralTrust;
namespace Vitqra
{
    public sealed class VitqraEphemeralTrustPreview:EditorWindow
    {
        private M fixture;
        private bool failed,document,signature,revision;
        private int day=25;
        private M.Revocation status=M.Revocation.Unknown;
        private static readonly Dictionary<string,string[]> Labels=new Dictionary<string,string[]>
        {
            {"en",new[]{"Ephemeral digital trust","Fictional exercise only. No real approval.","Generate local test","Verification day","Revocation status","Change document","Change signature","Wrong revision","Technical result","Cryptography unavailable"}},
            {"ca",new[]{"Confiança digital efímera","Només prova fictícia. Sense aprovació real.","Genera prova local","Dia de verificació","Estat de revocació","Modifica document","Modifica signatura","Revisió incorrecta","Resultat tècnic","Criptografia no disponible"}},
            {"es",new[]{"Confianza digital efímera","Solo prueba ficticia. Sin aprobación real.","Generar prueba local","Día de verificación","Estado de revocación","Modificar documento","Modificar firma","Revisión incorrecta","Resultado técnico","Criptografía no disponible"}},
            {"zh",new[]{"临时数字信任","仅虚构测试。无真实批准。","生成本地测试","验证日","撤销状态","修改文档","修改签名","错误版本","技术结果","加密不可用"}},
            {"hi",new[]{"अस्थायी डिजिटल विश्वास","केवल काल्पनिक परीक्षण। वास्तविक स्वीकृति नहीं।","स्थानीय परीक्षण बनाएँ","सत्यापन दिन","निरस्तीकरण स्थिति","दस्तावेज़ बदलें","हस्ताक्षर बदलें","गलत संशोधन","तकनीकी परिणाम","क्रिप्टोग्राफी अनुपलब्ध"}},
            {"fr",new[]{"Confiance numérique éphémère","Test fictif uniquement. Aucune approbation réelle.","Générer un test local","Jour de vérification","État de révocation","Modifier document","Modifier signature","Mauvaise révision","Résultat technique","Cryptographie indisponible"}},
            {"de",new[]{"Flüchtiges digitales Vertrauen","Nur fiktiver Test. Keine echte Freigabe.","Lokalen Test erzeugen","Prüftag","Widerrufsstatus","Dokument ändern","Signatur ändern","Falsche Revision","Technisches Ergebnis","Kryptografie nicht verfügbar"}},
            {"pt",new[]{"Confiança digital efémera","Apenas teste fictício. Sem aprovação real.","Gerar teste local","Dia de verificação","Estado de revogação","Alterar documento","Alterar assinatura","Revisão incorreta","Resultado técnico","Criptografia indisponível"}},
            {"ar",new[]{"ثقة رقمية مؤقتة","اختبار خيالي فقط. لا اعتماد حقيقي.","إنشاء اختبار محلي","يوم التحقق","حالة الإلغاء","تعديل المستند","تعديل التوقيع","مراجعة خاطئة","النتيجة التقنية","التشفير غير متاح"}},
            {"ja",new[]{"一時的なデジタル信頼","架空テストのみ。実際の承認なし。","ローカルテスト生成","検証日","失効状態","文書を変更","署名を変更","誤った改訂","技術結果","暗号機能が利用できません"}},
        };
        [MenuItem("VITQRA/Ephemeral Digital Trust Preview")]
        private static void Open()=>GetWindow<VitqraEphemeralTrustPreview>();
        private void OnGUI()
        {
            var l=Labels[VitqraStrings.DeviceLanguage];titleContent=new GUIContent(l[0]);
            EditorGUILayout.HelpBox(l[1],MessageType.Warning);
            if(GUILayout.Button(l[2]))
            {
                fixture=null;failed=false;
                try{fixture=M.Create();}
                catch(Exception){failed=true;}
            }
            if(failed)EditorGUILayout.HelpBox(l[9],MessageType.Warning);
            if(fixture==null)return;
            day=EditorGUILayout.IntSlider(l[3],day,0,180);
            status=(M.Revocation)EditorGUILayout.EnumPopup(l[4],status);
            document=EditorGUILayout.Toggle(l[5],document);
            signature=EditorGUILayout.Toggle(l[6],signature);
            revision=EditorGUILayout.Toggle(l[7],revision);
            EditorGUILayout.LabelField(l[8],fixture.Check(day,status,document,signature,revision).ToString());
        }
        private void OnDisable(){fixture=null;}
    }
}
