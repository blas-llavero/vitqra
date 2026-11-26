using System;
using UnityEditor;
using UnityEngine;

namespace Vitqra
{
    public static class VitqraDirectoryChecks
    {
        [MenuItem("VITQRA/Run Challenge Directory Checks")]
        public static void Run()
        {
            var items=VitqraChallengeDirectory.Educational();
            Check(items.Count==12,"Direct curated problems");
            Check(items.TrueForAll(i=>i.educational&&VitqraChallengeDirectory.ValidLink(i)),"Educational links");
            Check(VitqraChallengeDirectory.Filter(items,0,"").Count==12,"Bioinformatics area");
            Check(VitqraChallengeDirectory.Filter(items,3,"").Count==0,"Empty unrelated area");
            var matches=VitqraChallengeDirectory.Filter(items,-1,"COUNTING");
            Check(matches.Count==2&&matches.Exists(i=>i.id=="rosalind-dna")&&
                matches.Exists(i=>i.id=="rosalind-hamm"),"Keyword matches both curated titles");
            items.Add(items[0]);
            Check(VitqraChallengeDirectory.Filter(items,-1,"").Count==12,"Duplicates");
            Debug.Log("VITQRA directory checks passed.");
        }
        private static void Check(bool value,string name){if(!value)throw new InvalidOperationException(name);}
    }
}
