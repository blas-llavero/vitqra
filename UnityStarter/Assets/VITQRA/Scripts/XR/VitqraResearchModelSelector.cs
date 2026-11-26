using System;
using System.Collections.Generic;

namespace Vitqra
{
    // Research-first selection policy, reviewed 2026-11-26.
    // Exact documented model IDs only; the live authorized catalog is the
    // eligibility boundary. Never infer capabilities from a display name,
    // a version-like string, a price, or the order returned by the server.
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
        public static string Resolve(IEnumerable<VitqraChatAccount.Model> models,string manualSlug)
        {
            if(models==null)return "";
            var available=new HashSet<string>(StringComparer.Ordinal);
            foreach(var model in models)
                if(model!=null&&!string.IsNullOrEmpty(model.slug))available.Add(model.slug);
            if(!string.IsNullOrEmpty(manualSlug))
                return available.Contains(manualSlug)?manualSlug:"";
            foreach(string slug in Priority)
                if(available.Contains(slug))return slug;
            // Unknown models remain manually selectable. Do not guess their
            // capabilities or silently select an unrelated specialized model.
            return "";
        }
    }
}
