using System;
using System.Collections.Generic;

namespace Vitqra
{
    // Research-first selection policy, reviewed 2026-11-26.
    // The live authorized catalog is the eligibility boundary. Prefer exact
    // unique display-name matches to the official research ranking when its
    // bounded parser succeeds; otherwise use reviewed exact model IDs.
    // Never infer capabilities from versions, prices or catalog ordering.
    public static class VitqraResearchModelSelector
    {
        // Relative preference, not measured benchmark scores. The first four
        // current flagship models follow the official research task guidance.
        // Older documented models have a provisional research-first ordering.
        private static readonly string[] Priority={
            "gpt-6.2-astra","gpt-6-astra","gpt-6.2-sol","gpt-6.1-sol","gpt-6-sol",
            "gpt-6.1-terra","gpt-6-luna","gpt-5.6-sol","gpt-5.5-pro","gpt-5.5",
            "gpt-5.6-terra","gpt-5.4-pro","gpt-5.4","gpt-5.6-luna","gpt-5.4-mini",
            "gpt-5.2-pro","gpt-5.2"
        };
        public static string CurrentAutomaticSlug=>VitqraChatAccount.CatalogState==2?Resolve(VitqraChatAccount.Catalog,""):"";
        public static string Resolve(IEnumerable<VitqraChatAccount.Model> models,string manualSlug)
        {
            return Resolve(models,manualSlug,VitqraResearchModelGuidance.Current());
        }
        public static string Resolve(IEnumerable<VitqraChatAccount.Model> models,string manualSlug,IEnumerable<string> officialRanking)
        {
            if(models==null)return "";
            var available=new HashSet<string>(StringComparer.Ordinal);
            var names=new Dictionary<string,HashSet<string>>(StringComparer.Ordinal);
            foreach(var model in models)
            {
                if(model==null||string.IsNullOrEmpty(model.slug))continue;
                available.Add(model.slug);
                if(string.IsNullOrEmpty(model.name))continue;
                if(!names.TryGetValue(model.name,out var ids)){ids=new HashSet<string>(StringComparer.Ordinal);names.Add(model.name,ids);}
                ids.Add(model.slug);
            }
            if(!string.IsNullOrEmpty(manualSlug))
                return available.Contains(manualSlug)?manualSlug:"";
            if(officialRanking!=null)
                foreach(string name in officialRanking)
                    if(!string.IsNullOrEmpty(name)&&names.TryGetValue(name,out var ids)&&ids.Count==1)
                        foreach(string slug in ids)return slug;
            foreach(string slug in Priority)
                if(available.Contains(slug))return slug;
            // Unknown models remain manually selectable. Do not guess their
            // capabilities or silently select an unrelated specialized model.
            return "";
        }
    }
}
