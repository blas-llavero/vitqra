using System.Collections.Generic;
using UnityEngine;

namespace Vitqra
{
    public static class VitqraStrings
    {
        public static readonly string[] Languages = { "device", "en", "es", "ca", "zh", "hi", "fr", "de", "pt", "ar", "ja" };
        private static readonly Dictionary<string,string[]> Strings = new Dictionary<string,string[]>
        {
            { "en", new[] { "Desktop preview — no network access", "Language", "Automatic research", "Automatic translation", "AI provider", "Not connected — paid budget: 0", "Save settings", "Research challenges", "Search", "Live search is not implemented.", "Sample notes — no sensitive data", "Save note", "Load note", "Delete note", "Bioinformatics", "Nanoscience", "Cancer research", "Longevity", "Hackathons" } },
            { "es", new[] { "Vista previa de escritorio — sin acceso a la red", "Idioma", "Investigación automática", "Traducción automática", "Proveedor de IA", "Sin conexión — presupuesto de pago: 0", "Guardar ajustes", "Retos de investigación", "Buscar", "La búsqueda en línea aún no está implementada.", "Notas de prueba — sin datos sensibles", "Guardar nota", "Recuperar nota", "Eliminar nota", "Bioinformática", "Nanociencia", "Investigación del cáncer", "Longevidad", "Hackatones" } },
            { "ca", new[] { "Previsualització d'escriptori — sense accés a la xarxa", "Idioma", "Recerca automàtica", "Traducció automàtica", "Proveïdor d'IA", "Sense connexió — pressupost de pagament: 0", "Desar configuració", "Reptes de recerca", "Cercar", "La cerca en línia encara no està implementada.", "Notes de prova — sense dades sensibles", "Desar nota", "Recuperar nota", "Eliminar nota", "Bioinformàtica", "Nanociència", "Recerca del càncer", "Longevitat", "Hackathons" } },
            { "zh", new[] { "桌面预览 — 无网络访问", "语言", "自动研究", "自动翻译", "AI 提供商", "未连接 — 付费预算：0", "保存设置", "研究挑战", "搜索", "尚未实现在线搜索。", "示例笔记 — 不含敏感数据", "保存笔记", "读取笔记", "删除笔记", "生物信息学", "纳米科学", "癌症研究", "长寿", "黑客松" } },
            { "hi", new[] { "डेस्कटॉप पूर्वावलोकन — नेटवर्क उपलब्ध नहीं", "भाषा", "स्वचालित शोध", "स्वचालित अनुवाद", "एआई प्रदाता", "कनेक्ट नहीं — भुगतान बजट: 0", "सेटिंग सहेजें", "शोध चुनौतियाँ", "खोजें", "ऑनलाइन खोज अभी उपलब्ध नहीं है।", "नमूना नोट — संवेदनशील डेटा नहीं", "नोट सहेजें", "नोट पढ़ें", "नोट हटाएँ", "जैव सूचना विज्ञान", "नैनो विज्ञान", "कैंसर अनुसंधान", "दीर्घायु", "हैकाथॉन" } },
            { "fr", new[] { "Aperçu bureau — sans accès réseau", "Langue", "Recherche automatique", "Traduction automatique", "Fournisseur IA", "Non connecté — budget payant : 0", "Enregistrer les paramètres", "Défis de recherche", "Rechercher", "La recherche en ligne n'est pas encore disponible.", "Notes de test — aucune donnée sensible", "Enregistrer la note", "Charger la note", "Supprimer la note", "Bio-informatique", "Nanosciences", "Recherche sur le cancer", "Longévité", "Hackathons" } },
            { "de", new[] { "Desktop-Vorschau — kein Netzwerkzugriff", "Sprache", "Automatische Forschung", "Automatische Übersetzung", "KI-Anbieter", "Nicht verbunden — Kostenbudget: 0", "Einstellungen speichern", "Forschungsaufgaben", "Suchen", "Die Online-Suche ist noch nicht implementiert.", "Testnotizen — keine sensiblen Daten", "Notiz speichern", "Notiz laden", "Notiz löschen", "Bioinformatik", "Nanowissenschaften", "Krebsforschung", "Langlebigkeit", "Hackathons" } },
            { "pt", new[] { "Prévia de ambiente de trabalho — sem rede", "Idioma", "Pesquisa automática", "Tradução automática", "Fornecedor de IA", "Sem ligação — orçamento pago: 0", "Guardar definições", "Desafios de investigação", "Pesquisar", "A pesquisa online ainda não está disponível.", "Notas de teste — sem dados sensíveis", "Guardar nota", "Carregar nota", "Eliminar nota", "Bioinformática", "Nanociência", "Investigação do cancro", "Longevidade", "Hackathons" } },
            { "ar", new[] { "معاينة سطح المكتب — دون اتصال بالشبكة", "اللغة", "البحث التلقائي", "الترجمة التلقائية", "مزود الذكاء الاصطناعي", "غير متصل — ميزانية الدفع: 0", "حفظ الإعدادات", "تحديات البحث", "بحث", "البحث عبر الإنترنت غير متاح بعد.", "ملاحظات تجريبية — دون بيانات حساسة", "حفظ الملاحظة", "استرجاع الملاحظة", "حذف الملاحظة", "المعلوماتية الحيوية", "علوم النانو", "أبحاث السرطان", "طول العمر", "الهاكاثون" } },
            { "ja", new[] { "デスクトッププレビュー — ネットワーク接続なし", "言語", "自動研究", "自動翻訳", "AI プロバイダー", "未接続 — 有料予算：0", "設定を保存", "研究課題", "検索", "オンライン検索は未実装です。", "試用メモ — 機密情報なし", "メモを保存", "メモを読み込む", "メモを削除", "バイオインフォマティクス", "ナノ科学", "がん研究", "長寿", "ハッカソン" } }
        };
        public static string ResolveLanguage(string language)
        {
            if (language == "device")
            {
                switch (Application.systemLanguage.ToString())
                {
                    case "Spanish": language = "es"; break;
                    case "Catalan": language = "ca"; break;
                    case "Chinese": case "ChineseSimplified": case "ChineseTraditional": language = "zh"; break;
                    case "Hindi": language = "hi"; break;
                    case "French": language = "fr"; break;
                    case "German": language = "de"; break;
                    case "Portuguese": language = "pt"; break;
                    case "Arabic": language = "ar"; break;
                    case "Japanese": language = "ja"; break;
                    default: language = "en"; break;
                }
            }
            return language != null && Strings.ContainsKey(language) ? language : "en";
        }

        public static string[] Get(string language) => Strings[ResolveLanguage(language)];
    }
}
