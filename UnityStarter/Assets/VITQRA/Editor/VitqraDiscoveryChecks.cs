using System;
using UnityEditor;
using UnityEngine;

namespace Vitqra
{
    public static class VitqraDiscoveryChecks
    {
        [MenuItem("VITQRA/Run Discovery Checks")]
        public static void Run()
        {
            // Synthetic parser fixtures only; never shown as actual opportunities.
            string json="{\"count\":3,\"results\":[{\"document_number\":\"fixture-1\",\"title\":\"Synthetic bioinformatics challenge\",\"type\":\"Notice\",\"publication_date\":\"2025-01-01\",\"html_url\":\"https://www.federalregister.gov/documents/fixture\"},{\"document_number\":\"fixture-2\",\"title\":\"Synthetic challenge\",\"type\":\"Notice\",\"publication_date\":\"2025-01-01\",\"html_url\":\"https://example.invalid/documents/fixture\"},{\"document_number\":\"fixture-3\",\"title\":\"Synthetic unrelated rule\",\"type\":\"Notice\",\"publication_date\":\"2025-01-01\",\"html_url\":\"https://www.federalregister.gov/documents/fixture\"}]}";
            var batch=VitqraDiscovery.Parse(json);
            Check(batch.success&&batch.items.Count==1,"Reject foreign URLs and unrelated titles");
            batch.items.Add(batch.items[0]);
            Check(VitqraDiscovery.Filter(batch.items,"BIOINFORMATICS").Count==1,"Keyword and deduplication");
            Check(VitqraDiscovery.Filter(batch.items,"not-present").Count==0,"Empty results");
            Check(!VitqraDiscovery.Parse("not JSON").success,"Malformed data");
            Check(!VitqraDiscovery.ValidSource("https://www.federalregister.gov.example.invalid/documents/x"),"Host validation");
            Check(!VitqraDiscovery.ValidSource("http://www.federalregister.gov/documents/x"),"HTTPS");
            bool rejected=false;
            try{VitqraDiscovery.BuildUrl("private query");}catch(ArgumentException){rejected=true;}
            Check(rejected,"Only fixed public terms sent");
            Debug.Log("VITQRA discovery checks passed.");
        }
        private static void Check(bool value,string name){if(!value)throw new InvalidOperationException(name);}
    }
}
