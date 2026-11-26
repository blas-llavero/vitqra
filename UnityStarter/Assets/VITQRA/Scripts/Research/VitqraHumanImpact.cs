using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Vitqra
{
    // Research questions only. No model text can set an evidence grade,
    // certify safety, score patients, authorize treatment or release content.
    public static class VitqraHumanImpact
    {
        public const string Protocol="human-impact-questions-v1";
        public const string Prompt=" After the critique, give exactly one short line for each of these ASCII keys followed by = and a research question or uncertainty (max 240 characters per value, in the requested language): BENEFIT, HARM, QUALITY_OF_LIFE, ACCESS_EQUITY, COST, PRIVACY, MITIGATION, EVIDENCE. Use UNKNOWN if unsupported. No clinical instructions, numerical risk estimates, safety verdicts or personal data. Preserve any required CANDIDATE first line. These are hypotheses, never demonstrated impacts.";
        private static readonly string[] Keys={"BENEFIT","HARM","QUALITY_OF_LIFE","ACCESS_EQUITY","COST","PRIVACY","MITIGATION","EVIDENCE"};
        private static readonly Dictionary<string,string[]> Labels=new Dictionary<string,string[]>
        {
            {"en",new[]{"Human impact — research assessment","Unverified AI research questions, not established effects. No benefit, safety or clinical effectiveness is established. Independent evidence and qualified review are required.","Not assessed / unknown","Potential benefit","Potential harms and adverse effects","Quality of life and burden","Accessibility and equity","Cost and resources","Privacy and data protection","Risk mitigation","Missing evidence and validation"}},
            {"ca",new[]{"Impacte humà — avaluació de recerca","Preguntes de recerca de la IA sense validar, no efectes demostrats. No s'ha establert cap benefici, seguretat ni eficàcia clínica. Calen evidències independents i revisió qualificada.","No avaluat / desconegut","Benefici potencial","Riscos i efectes adversos potencials","Qualitat de vida i càrrega","Accessibilitat i equitat","Cost i recursos","Privadesa i protecció de dades","Reducció dels riscos","Evidències i validació pendents"}},
            {"es",new[]{"Impacto humano — evaluación de investigación","Preguntas de investigación de IA sin validar, no efectos demostrados. No se ha establecido beneficio, seguridad ni eficacia clínica. Se requieren evidencias independientes y revisión cualificada.","No evaluado / desconocido","Beneficio potencial","Riesgos y efectos adversos potenciales","Calidad de vida y carga","Accesibilidad y equidad","Coste y recursos","Privacidad y protección de datos","Mitigación de riesgos","Evidencias y validación pendientes"}},
            {"zh",new[]{"人类影响 — 研究评估","未经验证的 AI 研究问题，而非已证实的影响。尚未确定获益、安全性或临床有效性。需要独立证据和专业审查。","未评估 / 未知","潜在获益","潜在风险和不良影响","生活质量与负担","可及性与公平","成本与资源","隐私和数据保护","风险缓解","缺失证据与验证"}},
            {"hi",new[]{"मानव प्रभाव — शोध आकलन","असत्यापित AI शोध प्रश्न, सिद्ध प्रभाव नहीं। लाभ, सुरक्षा या नैदानिक प्रभावशीलता स्थापित नहीं है। स्वतंत्र प्रमाण और योग्य समीक्षा आवश्यक हैं।","आकलन नहीं / अज्ञात","संभावित लाभ","संभावित जोखिम और प्रतिकूल प्रभाव","जीवन की गुणवत्ता और बोझ","सुलभता और समानता","लागत और संसाधन","गोपनीयता और डेटा सुरक्षा","जोखिम कम करना","अनुपलब्ध प्रमाण और सत्यापन"}},
            {"fr",new[]{"Impact humain — évaluation de recherche","Questions de recherche IA non validées, et non effets démontrés. Aucun bénéfice, sécurité ou efficacité clinique n’est établi. Des preuves indépendantes et une revue qualifiée sont nécessaires.","Non évalué / inconnu","Bénéfice potentiel","Risques et effets indésirables potentiels","Qualité de vie et charge","Accessibilité et équité","Coûts et ressources","Confidentialité et protection des données","Réduction des risques","Preuves et validation manquantes"}},
            {"de",new[]{"Auswirkungen auf Menschen — Forschungsbewertung","Ungeprüfte KI-Forschungsfragen, keine nachgewiesenen Wirkungen. Nutzen, Sicherheit und klinische Wirksamkeit sind nicht belegt. Unabhängige Evidenz und qualifizierte Prüfung sind erforderlich.","Nicht bewertet / unbekannt","Möglicher Nutzen","Mögliche Risiken und Nebenwirkungen","Lebensqualität und Belastung","Zugänglichkeit und Chancengleichheit","Kosten und Ressourcen","Privatsphäre und Datenschutz","Risikominderung","Fehlende Evidenz und Validierung"}},
            {"pt",new[]{"Impacto humano — avaliação de investigação","Questões de investigação de IA não validadas, não efeitos demonstrados. Não foi estabelecido benefício, segurança ou eficácia clínica. São necessárias evidências independentes e revisão qualificada.","Não avaliado / desconhecido","Benefício potencial","Riscos e efeitos adversos potenciais","Qualidade de vida e carga","Acessibilidade e equidade","Custos e recursos","Privacidade e proteção de dados","Mitigação de riscos","Evidências e validação em falta"}},
            {"ar",new[]{"الأثر على الإنسان — تقييم بحثي","أسئلة بحثية غير متحققة من الذكاء الاصطناعي وليست آثارًا مثبتة. لم تثبت الفائدة أو السلامة أو الفعالية السريرية. يلزم دليل مستقل ومراجعة مؤهلة.","غير مقيّم / غير معروف","الفائدة المحتملة","المخاطر والآثار الضارة المحتملة","جودة الحياة والعبء","إمكانية الوصول والإنصاف","التكلفة والموارد","الخصوصية وحماية البيانات","الحد من المخاطر","الأدلة والتحقق الناقصان"}},
            {"ja",new[]{"人への影響 — 研究評価","未検証の AI 研究上の問いであり、実証された効果ではありません。利益、安全性、臨床的有効性は確立されていません。独立した証拠と専門家の審査が必要です。","未評価 / 不明","潜在的な利益","潜在的なリスクと有害作用","生活の質と負担","利用しやすさと公平性","費用と資源","プライバシーとデータ保護","リスク軽減","不足する証拠と検証"}},
        };
        private static string[] L(string language)=>Labels[VitqraStrings.ResolveLanguage(language)];
        public static string Heading(string language)=>L(language)[0];

        public static string[] Parse(string text)
        {
            var result=new string[Keys.Length];
            if(string.IsNullOrEmpty(text)||text.Length>6000)return result;
            var seen=new bool[Keys.Length];
            foreach(string raw in text.Split('\n'))
            {
                string line=raw.TrimEnd('\r');
                int equals=line.IndexOf('=');
                if(equals<=0)continue;
                int key=Array.IndexOf(Keys,line.Substring(0,equals));
                if(key<0)continue;
                // Duplicate fields are ambiguous, even when values are equal.
                if(seen[key]){result[key]=null;continue;}
                seen[key]=true;
                string value=line.Substring(equals+1).Trim();
                if(value.Length==0||value.Length>240||value=="UNKNOWN")continue;
                bool safe=true;
                foreach(char ch in value)
                {
                    var category=char.GetUnicodeCategory(ch);
                    if(char.IsControl(ch)||category==UnicodeCategory.Format||
                        ch=='<'||ch=='>'){safe=false;break;}
                }
                if(safe)result[key]=value;
            }
            return result;
        }
        public static string Describe(VitqraResearchDraft.Draft draft,string language)
        {
            var labels=L(language);
            // Older drafts did not receive the structured prompt. Never
            // retrospectively claim that these questions were assessed.
            var values=Parse(draft!=null&&draft.impactProtocol==Protocol?draft.critique:null);
            var body=new StringBuilder(labels[1]);
            for(int i=0;i<Keys.Length;i++)
                body.Append("\n\n").Append(labels[i+3]).Append(": ").Append(values[i]??labels[2]);
            if(draft!=null&&draft.impactProtocol==Protocol)
                body.Append("\n\n").Append("SHA-256 (draft): ").Append(draft.draftSha256);
            return body.ToString();
        }
    }
}
