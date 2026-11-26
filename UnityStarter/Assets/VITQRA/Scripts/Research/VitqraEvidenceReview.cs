using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Vitqra
{
    // Evidence review is not scientific peer review. All external content remains
    // untrusted data. This class never authorizes an autonomous research job.
    public static class VitqraEvidenceReview
    {
        public enum State { Unsupported, Missing, Inconsistent, Partial, DocumentChecksPassed }
        public enum SampleState { NotApplicable, Missing, NotAssessed, FormatMatches, FormatMismatch }
        public sealed class Report
        {
            public State state;
            public SampleState sample;
            public bool sourceIdentityMatches, manifestMatches, derivedTextMatches, pdfSignatureMatches;
            public bool eligibilityVerified => false;
            public bool deadlineVerified => false;
            public bool rightsVerified => false;
            public bool scientificValidationComplete => false;
            public bool CanStartAutonomousResearch => false;
        }

        public static bool SourceIdentityMatches(VitqraChallengeDirectory.Item item)
        {
            if(!VitqraEvidence.Eligible(item))return false;
            if(item.educational)
                return VitqraChallengeDirectory.Educational().Any(x=>x.id==item.id&&x.url==item.url);
            if(!DateTime.TryParseExact(item.publicationDate,"yyyy-MM-dd",CultureInfo.InvariantCulture,
                DateTimeStyles.None,out var published)||!item.id.StartsWith("fr-",StringComparison.Ordinal))return false;
            var uri=new Uri(item.url);
            if(!string.IsNullOrEmpty(uri.Fragment))return false;
            string route="/documents/"+published.ToString("yyyy/MM/dd",CultureInfo.InvariantCulture)+"/"+
                item.id.Substring(3)+"/";
            return uri.AbsolutePath.StartsWith(route,StringComparison.Ordinal)&&uri.AbsolutePath.Length>route.Length;
        }

        // Only known introductory sample formats are assessed. Other samples
        // remain unassessed, and no input is interpreted as a patient dataset.
        public static SampleState CheckSample(string id,string sample)
        {
            if(string.IsNullOrWhiteSpace(sample))return SampleState.Missing;
            int expected;
            switch(id)
            {
                case "rosalind-dna": case "rosalind-rna": case "rosalind-revc": expected=1;break;
                case "rosalind-hamm": case "rosalind-subs": expected=2;break;
                default:return SampleState.NotAssessed;
            }
            var lines=sample.Replace("\r\n","\n").Split('\n');
            if(lines.Length!=expected||lines.Any(x=>x.Length==0||x.Any(c=>c!='A'&&c!='C'&&c!='G'&&c!='T')))
                return SampleState.FormatMismatch;
            if(id=="rosalind-hamm"&&lines[0].Length!=lines[1].Length)return SampleState.FormatMismatch;
            if(id=="rosalind-subs"&&lines[1].Length>lines[0].Length)return SampleState.FormatMismatch;
            return SampleState.FormatMatches;
        }

        private static string ReadText(string path,int max)
        {
            var info=new FileInfo(path);
            if(!info.Exists||info.Length<=0||info.Length>max)throw new IOException("Evidence size outside limits");
            return new UTF8Encoding(false,true).GetString(File.ReadAllBytes(path));
        }

        public static Report Inspect(string storageRoot,VitqraChallengeDirectory.Item item)
        {
            var report=new Report {state=State.Unsupported};
            if(!VitqraEvidence.Eligible(item))return report;
            report.sourceIdentityMatches=SourceIdentityMatches(item);
            if(!report.sourceIdentityMatches){report.state=State.Inconsistent;return report;}
            string folder=VitqraEvidence.Folder(storageRoot,item.id,item.url);
            if(!Directory.Exists(folder)){report.state=State.Missing;return report;}
            report.state=State.Inconsistent;
            try
            {
                var m=VitqraEvidence.ReadVerified(folder,item);
                if(m==null||!DateTimeOffset.TryParseExact(m.retrievedUtc,"o",CultureInfo.InvariantCulture,
                    DateTimeStyles.None,out var retrieved)||retrieved>DateTimeOffset.UtcNow.AddMinutes(5)||
                    !string.Equals(m.title,item.title,StringComparison.Ordinal))return report;
                report.manifestMatches=true;
                var html=ReadText(Path.Combine(folder,"source.html.txt"),VitqraEvidence.MaxHtml);
                var preview=VitqraWebReader.Extract(html);
                var storedPreview=ReadText(Path.Combine(folder,"source-preview.txt"),VitqraWebReader.MaxText*4);
                report.derivedTextMatches=preview.success&&preview.truncated==m.previewTruncated&&
                    string.Equals(preview.text,storedPreview,StringComparison.Ordinal);
                if(!report.derivedTextMatches)return report;
                if(m.repositoryCandidates==null||!m.repositoryCandidates.SequenceEqual(VitqraEvidence.RepositoryLinks(html)))return report;
                foreach(var a in m.assets)
                {
                    string kind=a.name=="source.html.txt"?"original_html_bytes":
                        a.name=="source-preview.txt"?"derived_text_preview":
                        a.name=="public-sample.txt"?"educational_sample":"official_notice";
                    if(a.kind!=kind||a.rights!="source_terms_apply; redistribution_not_approved")return report;
                }
                bool partial=preview.truncated;
                if(item.educational)
                {
                    var expected=VitqraEvidence.SampleDataset(html);
                    bool saved=m.assets.Any(a=>a.name=="public-sample.txt");
                    if(saved)
                    {
                        var actual=ReadText(Path.Combine(folder,"public-sample.txt"),256000);
                        if(!string.Equals(expected,actual,StringComparison.Ordinal))return report;
                        report.sample=CheckSample(item.id,actual);
                        if(report.sample==SampleState.FormatMismatch)return report;
                        partial|=report.sample==SampleState.NotAssessed;
                    }
                    else
                    {
                        if(expected!=null)return report;
                        report.sample=SampleState.Missing;partial=true;
                    }
                }
                else
                {
                    report.sample=SampleState.NotApplicable;
                    if(m.assets.Any(a=>a.name=="official-notice.pdf"))
                    {
                        using(var f=File.OpenRead(Path.Combine(folder,"official-notice.pdf")))
                        {
                            var header=new byte[5];
                            report.pdfSignatureMatches=f.Read(header,0,header.Length)==header.Length&&
                                VitqraEvidence.PdfSignature(header);
                        }
                        if(!report.pdfSignatureMatches)return report;
                    }
                    else partial=true;
                }
                report.state=partial?State.Partial:State.DocumentChecksPassed;
                return report;
            }
            catch(Exception){return report;}
        }
    }
}
