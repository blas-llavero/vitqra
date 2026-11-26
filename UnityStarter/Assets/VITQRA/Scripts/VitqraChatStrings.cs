using System.Collections.Generic;

namespace Vitqra
{
    public static class VitqraChatStrings
    {
        public enum Key { Title, You, Assistant, Older, Newer, Write, Send, Voice, Minimize, NoVoice, NoAi, Prompt, Cancel, Done, Empty, Open, PreviewTip, Privacy }
        private static readonly Dictionary<string,string[]> Text = new Dictionary<string,string[]>
        {
            { "en", new[] { "Conversation", "You", "Assistant", "Older", "Newer", "Write", "Send", "Voice", "Minimize", "Voice not connected", "AI not connected — message saved in this session only.", "Write a message", "Cancel", "Done", "No messages", "Open conversation", "Preview tip", "No sensitive information" } },
            { "es", new[] { "Conversación", "Tú", "Asistente", "Anteriores", "Siguientes", "Escribir", "Enviar", "Voz", "Minimizar", "Voz sin conectar", "IA sin conectar: mensaje guardado solo en esta sesión.", "Escribe un mensaje", "Cancelar", "Aceptar", "Sin mensajes", "Abrir conversación", "Consejo de prueba", "Sin información sensible" } },
            { "ca", new[] { "Conversa", "Tu", "Assistent", "Anteriors", "Següents", "Escriure", "Enviar", "Veu", "Minimitzar", "Veu no connectada", "IA no connectada: missatge desat només en aquesta sessió.", "Escriu un missatge", "Cancel·lar", "Fet", "Sense missatges", "Obrir conversa", "Consell de prova", "Sense informació sensible" } },
            { "zh", new[] { "对话", "你", "助手", "较早", "较新", "输入", "发送", "语音", "最小化", "语音未连接", "AI 未连接：消息仅保存在本次会话。", "输入消息", "取消", "完成", "暂无消息", "打开对话", "示例建议", "请勿输入敏感信息" } },
            { "hi", new[] { "बातचीत", "आप", "सहायक", "पुराने", "नए", "लिखें", "भेजें", "आवाज़", "छोटा करें", "आवाज़ कनेक्ट नहीं", "एआई कनेक्ट नहीं — संदेश केवल इस सत्र में है।", "संदेश लिखें", "रद्द करें", "पूर्ण", "कोई संदेश नहीं", "बातचीत खोलें", "नमूना सलाह", "संवेदनशील जानकारी नहीं" } },
            { "fr", new[] { "Conversation", "Vous", "Assistant", "Précédents", "Suivants", "Écrire", "Envoyer", "Voix", "Réduire", "Voix non connectée", "IA non connectée : message conservé pour cette session seulement.", "Écrivez un message", "Annuler", "Terminé", "Aucun message", "Ouvrir la conversation", "Conseil de test", "Pas de données sensibles" } },
            { "de", new[] { "Unterhaltung", "Du", "Assistent", "Ältere", "Neuere", "Schreiben", "Senden", "Sprache", "Minimieren", "Sprache nicht verbunden", "KI nicht verbunden: Nachricht nur in dieser Sitzung gespeichert.", "Nachricht schreiben", "Abbrechen", "Fertig", "Keine Nachrichten", "Unterhaltung öffnen", "Beispielhinweis", "Keine sensiblen Daten" } },
            { "pt", new[] { "Conversa", "Tu", "Assistente", "Anteriores", "Seguintes", "Escrever", "Enviar", "Voz", "Minimizar", "Voz não ligada", "IA não ligada: mensagem guardada apenas nesta sessão.", "Escreve uma mensagem", "Cancelar", "Concluir", "Sem mensagens", "Abrir conversa", "Conselho de teste", "Sem dados sensíveis" } },
            { "ar", new[] { "المحادثة", "أنت", "المساعد", "الأقدم", "الأحدث", "كتابة", "إرسال", "صوت", "تصغير", "الصوت غير متصل", "الذكاء الاصطناعي غير متصل: الرسالة محفوظة في هذه الجلسة فقط.", "اكتب رسالة", "إلغاء", "تم", "لا توجد رسائل", "فتح المحادثة", "نصيحة تجريبية", "لا تدخل معلومات حساسة" } },
            { "ja", new[] { "会話", "あなた", "アシスタント", "以前", "次へ", "入力", "送信", "音声", "最小化", "音声未接続", "AI 未接続：メッセージはこのセッション内のみ保存されます。", "メッセージを入力", "キャンセル", "完了", "メッセージなし", "会話を開く", "サンプル助言", "機密情報を入力しないでください" } },
        };
        public static string Get(string language, Key key) => Text[VitqraStrings.ResolveLanguage(language)][(int)key];
    }
}
