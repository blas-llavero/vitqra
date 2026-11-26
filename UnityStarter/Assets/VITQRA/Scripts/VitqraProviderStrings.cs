using System.Collections.Generic;
namespace Vitqra
{
    public static class VitqraProviderStrings
    {
        public enum Key { Title, Local, ChatGpt, Ready, NotReady, Pending, Translate, Avatar, On, Off, Global }
        private static readonly Dictionary<string,string[]> Values=new Dictionary<string,string[]>
        {
            {"en",new[]{"AI settings","Local AI","ChatGPT (own account)","Reviewed GGUF model ready for verification","Local model not configured","Official account connection pending — no requests","Automatic translation","Avatar settings","On","Off","One provider for all challenges"}},
            {"es",new[]{"Configuración de IA","IA local","ChatGPT (cuenta propia)","Modelo GGUF revisado pendiente de verificación","Modelo local sin configurar","Conexión oficial pendiente — sin peticiones","Traducción automática","Configurar avatar","Sí","No","Un proveedor para todos los retos"}},
            {"ca",new[]{"Configuració de la IA","IA local","ChatGPT (compte propi)","Model GGUF revisat pendent de verificació","Model local sense configurar","Connexió oficial pendent — sense peticions","Traducció automàtica","Configurar avatar","Sí","No","Un proveïdor per a tots els reptes"}},
            {"zh",new[]{"AI 设置","本地 AI","ChatGPT（个人账户）","已审查的 GGUF 模型待验证","本地模型未配置","官方账户连接待实现 — 无请求","自动翻译","虚拟形象设置","开","关","所有挑战使用同一提供方"}},
            {"hi",new[]{"AI सेटिंग","स्थानीय AI","ChatGPT (अपना खाता)","समीक्षित GGUF मॉडल जाँच हेतु तैयार","स्थानीय मॉडल कॉन्फ़िगर नहीं","आधिकारिक खाता कनेक्शन लंबित — कोई अनुरोध नहीं","स्वचालित अनुवाद","अवतार सेटिंग","चालू","बंद","सभी चुनौतियों के लिए एक प्रदाता"}},
            {"fr",new[]{"Paramètres IA","IA locale","ChatGPT (compte personnel)","Modèle GGUF examiné à vérifier","Modèle local non configuré","Connexion officielle en attente — aucune requête","Traduction automatique","Paramètres avatar","Oui","Non","Un fournisseur pour tous les défis"}},
            {"de",new[]{"KI-Einstellungen","Lokale KI","ChatGPT (eigenes Konto)","Geprüftes GGUF-Modell zur Verifikation bereit","Lokales Modell nicht konfiguriert","Offizielle Kontoverbindung ausstehend — keine Anfragen","Automatische Übersetzung","Avatar-Einstellungen","An","Aus","Ein Anbieter für alle Aufgaben"}},
            {"pt",new[]{"Definições de IA","IA local","ChatGPT (conta própria)","Modelo GGUF revisto a verificar","Modelo local não configurado","Ligação oficial pendente — sem pedidos","Tradução automática","Configurar avatar","Sim","Não","Um fornecedor para todos os desafios"}},
            {"ar",new[]{"إعدادات الذكاء الاصطناعي","ذكاء اصطناعي محلي","ChatGPT (حساب شخصي)","نموذج GGUF مراجع بانتظار التحقق","النموذج المحلي غير مهيأ","ربط الحساب الرسمي معلق — لا طلبات","ترجمة تلقائية","إعدادات الصورة الرمزية","تشغيل","إيقاف","مزود واحد لكل التحديات"}},
            {"ja",new[]{"AI 設定","ローカル AI","ChatGPT（自分のアカウント）","レビュー済み GGUF モデルの検証待ち","ローカルモデル未設定","公式アカウント接続は未実装 — リクエストなし","自動翻訳","アバター設定","オン","オフ","全課題で同じプロバイダー"}},
        };
        public static string Get(string language,Key key)=>Values[VitqraStrings.ResolveLanguage(language)][(int)key];
    }
}
