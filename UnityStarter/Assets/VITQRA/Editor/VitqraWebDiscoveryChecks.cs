using System;
using UnityEditor;
using UnityEngine;

namespace Vitqra
{
    public static class VitqraWebDiscoveryChecks
    {
        [MenuItem("VITQRA/Run Web Discovery Checks")]
        public static void Run()
        {
            Check(VitqraWebDiscovery.ValidEndpoint("https://search.example.org/search"),"HTTPS endpoint");
            Check(!VitqraWebDiscovery.ValidEndpoint("http://search.example.org/search"),"Reject remote HTTP");
            Check(!VitqraWebDiscovery.ValidEndpoint("https://search.example.org/search?x=1"),"Reject configured query");
            Check(!VitqraWebDiscovery.ValidCandidateUrl("https://127.0.0.1/challenge"),"Reject IP");
            Check(!VitqraWebDiscovery.ValidCandidateUrl("https://server.local/challenge"),"Reject local host");
            Check(!VitqraWebDiscovery.ValidCandidateUrl("https://user@host.org/challenge"),"Reject credentials");
            Check(!VitqraWebDiscovery.ValidCandidateUrl("file:///tmp/test"),"Reject non-HTTPS");
            string url=VitqraWebDiscovery.BuildUrl("https://search.example.org/search","quantum & optics","ca","challenge");
            Check(url.Contains("quantum%20%26%20optics%20challenge"),"Encode query");
            string json="{\"results\":[{\"title\":\"Synthetic quantum challenge\",\"url\":\"https://research.example.org/challenges/fixture\",\"content\":\"Synthetic fixture\"},{\"title\":\"Synthetic challenge index\",\"url\":\"https://research.example.org/challenges/\"},{\"title\":\"Synthetic challenge\",\"url\":\"http://research.example.org/challenges/fixture\"}]}";
            var batch=VitqraWebDiscovery.Parse(json);
            Check(batch.success&&batch.items.Count==1&&batch.items[0].webCandidate,"Candidate classification");
            Check(!VitqraWebDiscovery.Parse("invalid JSON").success,"Malformed response");
            Debug.Log("VITQRA web discovery checks passed.");
        }
        private static void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);}
    }
}
