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
            Check(VitqraChallengeDirectory.Filter(items,-1,"Counting").Count==1,"Keyword");
            items.Add(items[0]);
            Check(VitqraChallengeDirectory.Filter(items,-1,"").Count==12,"Duplicates");
            Debug.Log("VITQRA directory checks passed.");
        }
        private static void Check(bool value,string name){if(!value)throw new InvalidOperationException(name);}
    }
}
