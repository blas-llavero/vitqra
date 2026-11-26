using System;
using System.Collections.Generic;
using System.Globalization;

namespace Vitqra
{
    public static class VitqraChallengeDirectory
    {
        public sealed class Item
        {
            public string id,title,url,keywords;
            public bool educational;
        }
        // Reviewed BIOQRA snapshot, direct problem links only. Educational exercises
        // are not open research competitions. Original titles and URLs are preserved.
        public static List<Item> Educational() => new List<Item>
        {
            new Item { id="rosalind-dna",title="Counting DNA Nucleotides",url="https://rosalind.info/problems/dna/",keywords="bioinformatics genetics sequences DNA RNA genome",educational=true },
            new Item { id="rosalind-rna",title="Transcribing DNA into RNA",url="https://rosalind.info/problems/rna/",keywords="bioinformatics genetics sequences DNA RNA genome",educational=true },
            new Item { id="rosalind-revc",title="Complementing a Strand of DNA",url="https://rosalind.info/problems/revc/",keywords="bioinformatics genetics sequences DNA RNA genome",educational=true },
            new Item { id="rosalind-gc",title="Computing GC Content",url="https://rosalind.info/problems/gc/",keywords="bioinformatics genetics sequences DNA RNA genome",educational=true },
            new Item { id="rosalind-hamm",title="Counting Point Mutations",url="https://rosalind.info/problems/hamm/",keywords="bioinformatics genetics sequences DNA RNA genome",educational=true },
            new Item { id="rosalind-subs",title="Finding a Motif in DNA",url="https://rosalind.info/problems/subs/",keywords="bioinformatics genetics motifs DNA RNA genome",educational=true },
            new Item { id="rosalind-cons",title="Consensus and Profile",url="https://rosalind.info/problems/cons/",keywords="bioinformatics genetics sequences DNA RNA genome",educational=true },
            new Item { id="rosalind-grph",title="Overlap Graphs",url="https://rosalind.info/problems/grph/",keywords="bioinformatics genetics genome-assembly DNA RNA genome",educational=true },
            new Item { id="rosalind-orf",title="Open Reading Frames",url="https://rosalind.info/problems/orf/",keywords="bioinformatics genetics gene-prediction DNA RNA genome",educational=true },
            new Item { id="rosalind-lcsm",title="Finding a Shared Motif",url="https://rosalind.info/problems/lcsm/",keywords="bioinformatics genetics motifs DNA RNA genome",educational=true },
            new Item { id="rosalind-ba5g",title="Compute the Edit Distance Between Two Strings",url="https://rosalind.info/problems/ba5g/",keywords="bioinformatics genetics alignment DNA RNA genome",educational=true },
            new Item { id="rosalind-ba3d",title="Construct the De Bruijn Graph of a String",url="https://rosalind.info/problems/ba3d/",keywords="bioinformatics genetics genome-assembly DNA RNA genome",educational=true },
        };
        public static readonly string[][] AreaTerms={
            new[]{"bioinformatics","genomics","genome","sequence","single cell","computational biology"},
            new[]{"genetics","genomic","genome","dna","rna","gene","mutation"},
            new[]{"biotechnology","biotech","biomanufacturing","synthetic biology"},
            new[]{"nanotechnology","nanoscience","nanomaterial","nanoscale"},
            new[]{"cancer","oncology","tumor","tumour","carcinoma"},
            new[]{"nanomedicine","nanoparticle","drug delivery","nanotherapeutic"},
            new[]{"artificial intelligence","machine learning","deep learning","computer vision","AI"},
            new[]{"longevity","aging","ageing","geroscience","healthspan"},
            new[]{"global","climate","sustainability","water","energy","public health"},
            new[]{"hackathon","hackathons"}
        };
        private static bool Contains(string text,string word)
        {
            if(string.Equals(word,"AI",StringComparison.Ordinal))
                return System.Text.RegularExpressions.Regex.IsMatch(text,@"\bAI\b",System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return CultureInfo.InvariantCulture.CompareInfo.IndexOf(text,word,CompareOptions.IgnoreCase|CompareOptions.IgnoreNonSpace)>=0;
        }
        public static List<Item> Filter(IEnumerable<Item> source,int area,string query)
        {
            var result=new List<Item>();
            var ids=new HashSet<string>(StringComparer.Ordinal);
            var terms=(query??"").Split(new[]{' ','\t','\n'},StringSplitOptions.RemoveEmptyEntries);
            foreach(var item in source)
            {
                if(item==null||!ValidLink(item)||string.IsNullOrWhiteSpace(item.title))continue;
                string searchable=item.title+" "+item.keywords;
                if(area>=0&&area<AreaTerms.Length&&!Array.Exists(AreaTerms[area],x=>Contains(searchable,x)))continue;
                if(!Array.TrueForAll(terms,x=>Contains(searchable,x)))continue;
                if(ids.Add(item.id))result.Add(item);
            }
            return result;
        }
        public static bool ValidLink(Item item)
        {
            if(item==null)return false;
            if(!item.educational)return VitqraDiscovery.ValidSource(item.url);
            return Uri.TryCreate(item.url,UriKind.Absolute,out var uri)&&uri.Scheme=="https"&&
                uri.Host=="rosalind.info"&&uri.IsDefaultPort&&string.IsNullOrEmpty(uri.UserInfo)&&
                uri.AbsolutePath.StartsWith("/problems/",StringComparison.Ordinal);
        }
    }
}
