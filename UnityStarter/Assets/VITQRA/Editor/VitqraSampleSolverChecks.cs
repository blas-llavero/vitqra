using System;
using UnityEditor;
using UnityEngine;
namespace Vitqra
{
    public static class VitqraSampleSolverChecks
    {
        [MenuItem("VITQRA/Run Educational Sample Checks")]
        public static void Run()
        {
            // Synthetic inputs for tests only, not retrieved data.
            Check(VitqraSampleSolver.Solve("rosalind-dna","AACGT")=="2 1 1 1","Counts");
            Check(VitqraSampleSolver.Solve("rosalind-rna","ACTT")=="ACUU","RNA");
            Check(VitqraSampleSolver.Solve("rosalind-revc","ACTG")=="CAGT","Reverse complement");
            Check(VitqraSampleSolver.Solve("rosalind-hamm","ACTG\nAGTA")=="2","Hamming");
            Check(VitqraSampleSolver.Solve("rosalind-subs","AAAA\nAA")=="1 2 3","Overlapping motifs");
            Check(VitqraSampleSolver.Solve("rosalind-dna","AX")==null,"Invalid input");
            Check(VitqraSampleSolver.Solve("rosalind-dna",new string('A',1001))==null,"Bounded input");
            Check(VitqraSampleSolver.Solve("rosalind-gc","ACTG")==null,"Unsupported");
            Check(VitqraSampleSolver.ExpectedOutput("<h2>Sample Output</h2><pre>1  2\n3</pre>")=="1 2 3","Output normalization");
            Check(VitqraSampleSolver.ExpectedOutput("<h2>Output</h2><pre>1</pre>")==null,"Unlabeled output");
            Check(VitqraSampleSolver.ExpectedOutput("<h2>Sample Output</h2><pre>1</pre><h2>Sample Output</h2><pre>2</pre>")==null,"Ambiguous output");
            Check(!new VitqraEvidenceReview.Report().CanStartAutonomousResearch,"Scientific gate remains closed");
            Debug.Log("VITQRA educational sample checks passed.");
        }
        private static void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);}
    }
}
