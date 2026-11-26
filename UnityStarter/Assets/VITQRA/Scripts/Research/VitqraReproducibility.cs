using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Vitqra
{
    // Local reproducibility record, never an independent approval or a release gate.
    public static class VitqraReproducibility
    {
        private static readonly UTF8Encoding Utf8=new UTF8Encoding(false,true);
        private static readonly Dictionary<string,string[]> Labels=new Dictionary<string,string[]>
        {
            {"en",new[]{"Reproducibility record","Local consistency only; independent scientific and clinical validation have not been performed. Original evidence remains separate."}},
            {"ca",new[]{"Registre de reproductibilitat","Només consistència local; no s'ha fet validació científica ni clínica independent. Les evidències originals es conserven per separat."}},
            {"es",new[]{"Registro de reproducibilidad","Solo consistencia local; no se ha realizado validación científica ni clínica independiente. Las evidencias originales se conservan por separado."}},
            {"zh",new[]{"可复现性记录","仅检查本地一致性；尚未进行独立科学或临床验证。原始证据单独保存。"}},
            {"hi",new[]{"पुनरुत्पादकता रिकॉर्ड","केवल स्थानीय संगति; स्वतंत्र वैज्ञानिक या नैदानिक सत्यापन नहीं हुआ है। मूल प्रमाण अलग रखे जाते हैं।"}},
            {"fr",new[]{"Registre de reproductibilité","Cohérence locale uniquement ; aucune validation scientifique ou clinique indépendante. Les preuves originales restent séparées."}},
            {"de",new[]{"Reproduzierbarkeitsprotokoll","Nur lokale Konsistenz; keine unabhängige wissenschaftliche oder klinische Validierung. Originalbelege bleiben getrennt."}},
            {"pt",new[]{"Registo de reprodutibilidade","Apenas consistência local; não foi realizada validação científica ou clínica independente. As evidências originais são conservadas separadamente."}},
            {"ar",new[]{"سجل قابلية إعادة الإنتاج","اتساق محلي فقط؛ لم يجر تحقق علمي أو سريري مستقل. تحفظ الأدلة الأصلية منفصلة."}},
            {"ja",new[]{"再現性記録","ローカル整合性のみ。独立した科学的・臨床的検証は未実施です。原証拠は別に保存します。"}},
        };
        public static string Heading(string language)=>Labels[VitqraStrings.ResolveLanguage(language)][0];
        public static string Scope(string language)=>Labels[VitqraStrings.ResolveLanguage(language)][1];
        public static string Hash(string text)
        {
            using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Utf8.GetBytes(text))).Replace("-","").ToLowerInvariant();
        }
        public static string EscapeValue(string value)
        {
            if(value==null||value.Length>4096)throw new InvalidDataException();
            var b=new StringBuilder();
            foreach(char ch in value)
            {
                if(ch=='%'||char.IsControl(ch)||char.GetUnicodeCategory(ch)==UnicodeCategory.Format||
                   ch=='\u2028'||ch=='\u2029')
                    b.Append('%').Append(((int)ch).ToString("X4",CultureInfo.InvariantCulture));
                else b.Append(ch);
            }
            return b.ToString();
        }
        public static string Status(bool present,bool checkedLocally)=>
            !present?"not_recorded":checkedLocally?"locally_rechecked":"inconsistent_or_unavailable";
        private static void Add(StringBuilder b,string key,string value)=>b.Append(key).Append('=').Append(EscapeValue(value)).Append('\n');
        public static string Build(string evidence,string results,string drafts,VitqraChallengeDirectory.Item item,string language)
        {
            try
            {
                if(!VitqraEvidence.Eligible(item))return null;
                var review=VitqraEvidenceReview.Inspect(evidence,item);
                if(review.state!=VitqraEvidenceReview.State.DocumentChecksPassed&&review.state!=VitqraEvidenceReview.State.Partial)return null;
                var manifest=VitqraEvidence.ReadVerified(VitqraEvidence.Folder(evidence,item.id,item.url),item);
                if(manifest==null)return null;
                string locale=VitqraStrings.ResolveLanguage(language);
                string folder=VitqraEvidence.Folder(results,item.id,item.url);
                var b=new StringBuilder();
                Add(b,"protocol","research-reproducibility-v1");
                Add(b,"challenge_id",item.id);
                Add(b,"source_url",manifest.sourceUrl);
                Add(b,"retrieved_utc",manifest.retrievedUtc);
                Add(b,"locale",locale);
                Add(b,"document_status",review.state.ToString());
                Add(b,"preview_truncated",manifest.previewTruncated?"true":"false");
                foreach(var asset in manifest.assets.OrderBy(x=>x.name,StringComparer.Ordinal))
                {
                    Add(b,"asset."+asset.name+".sha256",asset.sha256);
                    Add(b,"asset."+asset.name+".bytes",asset.bytes.ToString(CultureInfo.InvariantCulture));
                    Add(b,"asset."+asset.name+".source",asset.sourceUrl);
                }
                bool supported=item.educational&&VitqraSampleSolver.Supports(item.id);
                bool present=supported&&File.Exists(Path.Combine(folder,"sample-check.json"));
                bool checkedSample=present&&VitqraSampleSolver.Run(evidence,results,item)==VitqraSampleSolver.State.Checked;
                Add(b,"sample_status",supported?Status(present,checkedSample):"not_applicable");
                if(checkedSample)
                {
                    var input=manifest.assets.Single(x=>x.name=="public-sample.txt");
                    string sourceFolder=VitqraEvidence.Folder(evidence,item.id,item.url);
                    // Read the bounded input again; verify the actual bytes before recomputing.
                    string samplePath=Path.Combine(sourceFolder,"public-sample.txt");
                    string htmlPath=Path.Combine(sourceFolder,"source.html.txt");
                    string sample=Read(samplePath,8192),html=Read(htmlPath,VitqraEvidence.MaxHtml);
                    if(Hash(sample)!=input.sha256||Hash(html)!=manifest.assets.Single(x=>x.name=="source.html.txt").sha256)return null;
                    string expected=VitqraSampleSolver.ExpectedOutput(html),actual=VitqraSampleSolver.Solve(item.id,sample);
                    if(expected==null||actual==null||string.Join(" ",actual.Split((char[])null,StringSplitOptions.RemoveEmptyEntries))!=expected)return null;
                    Add(b,"sample_protocol","public_educational_sample_only_v1");
                    Add(b,"sample_expected_sha256",Hash(expected));
                    Add(b,"sample_result_sha256",Hash(actual));
                }
                bool motif=item.id==VitqraMotifBenchmark.ChallengeId;
                bool benchmarkPresent=motif&&File.Exists(Path.Combine(folder,"motif-benchmark-v1.txt"));
                string benchmark=benchmarkPresent?VitqraMotifBenchmark.Load(evidence,results,item):null;
                Add(b,"benchmark_status",motif?Status(benchmarkPresent,benchmark!=null):"not_applicable");
                if(benchmark!=null)Add(b,"benchmark_sha256",Hash(benchmark));
                var draft=VitqraResearchDraft.Load(evidence,drafts,item,locale);
                bool draftPresent=File.Exists(Path.Combine(VitqraEvidence.Folder(drafts,item.id,item.url),locale,"research-draft.json"));
                Add(b,"draft_status",draft==null?Status(draftPresent,false):"local_digest_checked_unvalidated");
                if(draft!=null)
                {
                    Add(b,"draft_sha256",draft.draftSha256);
                    Add(b,"model_identity",draft.ModelIdentity);
                    string revision=benchmark==null?null:VitqraMotifOptimization.Describe(draft);
                    if(revision!=null)Add(b,"revision_sha256",Hash(revision));
                }
                Add(b,"ai_independent_replication","not_performed");
                Add(b,"human_impact",draft!=null&&draft.impactProtocol==VitqraHumanImpact.Protocol?"unvalidated_questions_only":"not_assessed");
                Add(b,"eligibility","not_verified");
                Add(b,"deadline","not_verified");
                Add(b,"data_rights","not_verified");
                Add(b,"independent_scientific_validation","not_performed");
                Add(b,"clinical_validation","not_performed");
                Add(b,"clinical_release","blocked");
                Add(b,"autonomous_validated_research","blocked");
                Add(b,"record_body_sha256",Hash(b.ToString()));
                string record=b.ToString();
                return Utf8.GetByteCount(record)<=32768?record:null;
            }
            catch(Exception){return null;}
        }
        private static string Read(string path,int max)
        {
            using(var f=File.OpenRead(path))
            {
                if(f.Length<=0||f.Length>max)throw new IOException();
                var bytes=new byte[(int)f.Length];int n=0;
                while(n<bytes.Length){int k=f.Read(bytes,n,bytes.Length-n);if(k<=0)throw new IOException();n+=k;}
                if(f.ReadByte()!=-1)throw new IOException();
                return Utf8.GetString(bytes);
            }
        }
    }
}
