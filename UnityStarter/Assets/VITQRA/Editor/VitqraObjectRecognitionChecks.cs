using System;
using UnityEditor;
using UnityEngine;
using R=Vitqra.VitqraObjectRecognition;
namespace Vitqra
{
    public static class VitqraObjectRecognitionChecks
    {
        [MenuItem("VITQRA/Run Synthetic Object Recognition Checks")]
        public static void Run()
        {
            var x=new R();
            Check(x.RunFixture(R.Shape.Circle,6,0,0,0)==null&&x.Preview()==null,"Default disabled");
            x.SetEnabled(true);
            int[] noise={0,6,18,45};
            int[,,] expected={
                {{36,0,0,0},{0,36,0,0},{0,0,36,0}},
                {{36,0,0,0},{0,36,0,0},{0,0,36,0}},
                {{36,0,0,0},{0,36,0,0},{0,0,36,0}},
                {{16,0,0,20},{0,26,0,10},{0,0,2,34}},
            };
            int total=0;
            for(int n=0;n<noise.Length;n++)for(int k=0;k<3;k++)
            {
                var counts=new int[4];
                for(int r=5;r<=8;r++)foreach(int dx in new[]{-2,1,2})foreach(int dy in new[]{-2,1,2})
                {
                    var m=x.RunFixture((R.Shape)k,r,dx,dy,noise[n]);
                    Check(m!=null&&m.score>=0&&m.score<=1000&&m.margin>=0,"Score bounds");
                    counts[(int)m.label]++;total++;
                }
                for(int i=0;i<4;i++)Check(counts[i]==expected[n,k,i],"Reference matrix");
            }
            Check(total==432,"Known fixture count");
            for(int k=3;k<=6;k++)Check(x.RunFixture((R.Shape)k,6,0,0,0).label==R.Label.Unknown,"Unknown fixture");
            var a=x.RunFixture(R.Shape.Circle,6,0,0,0);
            var copy=x.Preview();copy[0]=!copy[0];
            Check(copy[0]!=x.Preview()[0]&&a.label==R.Label.Circle,"Defensive preview copy");
            Check(x.RunFixture(R.Shape.Circle,99,0,0,0)==null&&x.Current==null&&x.Preview()==null,"Invalid clears");
            x.RunFixture(R.Shape.Circle,6,0,0,0);x.SetEnabled(false);
            Check(x.Current==null&&x.Preview()==null,"Opt out clears");
            x.SetEnabled(true);Check(x.Current==null,"No old result revival");
            Check(!x.CameraEnabled&&!x.NetworkEnabled&&!x.ClinicalRelease,"Closed gates");
            Debug.Log("VITQRA synthetic object recognition checks passed.");
        }
        private static void Check(bool ok,string label){if(!ok)throw new InvalidOperationException(label);}
    }
}
