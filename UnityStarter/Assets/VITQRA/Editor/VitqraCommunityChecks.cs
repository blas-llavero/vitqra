using System;
using UnityEditor;
using UnityEngine;
using D=Vitqra.VitqraCommunityPolicy;
using A=Vitqra.VitqraCommunityPolicy.Actor;
using R=Vitqra.VitqraCommunityPolicy.Resource;
using P=Vitqra.VitqraCommunityPolicy.Purpose;
using O=Vitqra.VitqraCommunityPolicy.Action;

namespace Vitqra
{
    public static class VitqraCommunityChecks
    {
        private static readonly int[,] Expected={
            {0,0,0,0,0,90,181},
            {0,0,0,1,0,90,181},
            {1,1,0,0,0,90,181},
            {1,1,0,1,0,90,181},
            {2,0,0,0,10,70,50},
            {3,1,0,0,10,70,181},
            {4,2,1,0,5,65,181},
            {4,4,1,0,5,65,181},
            {5,4,2,0,15,60,181},
            {6,6,3,0,20,80,181},
            {6,6,3,2,20,80,181},
            {0,8,4,5,0,90,181},
            {0,8,4,0,0,90,181},
            {0,8,4,3,0,90,181},
            {7,8,4,5,0,90,181},
            {7,8,4,0,0,90,181},
            {7,8,4,3,0,90,181},
            {8,8,4,0,0,90,181},
            {8,8,4,4,0,90,181},
            {0,10,0,5,20,30,181},
            {2,10,0,5,20,30,181},
            {1,11,0,5,20,30,181},
            {3,11,0,5,20,30,181},
            {0,12,5,0,0,181,181},
            {1,12,5,0,0,181,181},
            {7,12,5,0,0,181,181},
        };
        [MenuItem("VITQRA/Run Synthetic Community Policy Checks")]
        public static void Run()
        {
            int[] days={0,4,5,9,10,14,15,19,20,29,30,49,50,59,60,64,65,69,70,79,80,89,90,180};
            int count=0,allowed=0;
            for(int a=0;a<12;a++)for(int r=0;r<13;r++)for(int p=0;p<6;p++)for(int o=0;o<10;o++)foreach(int day in days)
            {
                bool expected=false;
                for(int i=0;i<Expected.GetLength(0);i++)
                    if(a==Expected[i,0]&&r==Expected[i,1]&&p==Expected[i,2]&&o==Expected[i,3]&&
                        day>=Expected[i,4]&&day<Expected[i,5]&&day<Expected[i,6]){expected=true;break;}
                var d=D.Evaluate((A)a,(R)r,(P)p,(O)o,day);
                Check((d.outcome==D.Outcome.AllowSimulation)==expected,"Exact resource scope");
                count++;if(expected)allowed++;
            }
            Check(count==224640&&allowed==426,"Reference counts");
            Check(D.Evaluate(A.ClinicianA,R.RecordA,P.Care,O.Read,9).reason==D.Reason.NotYetActive,"Not active");
            Check(D.Evaluate(A.ClinicianA,R.RecordA,P.Care,O.Read,50).reason==D.Reason.Revoked,"Revocation");
            Check(D.Evaluate(A.ClinicianB,R.RecordB,P.Care,O.Read,70).reason==D.Reason.Expired,"Expiry");
            Check(D.Evaluate(A.ClinicianA,R.RecordB,P.Care,O.Read,25).reason==D.Reason.NoResourceGrant,"Other patient");
            Check(D.Evaluate((A)999,R.RecordA,P.Care,O.Read,25).reason==D.Reason.Invalid,"Invalid identity");
            Check(D.Evaluate(A.PatientA,R.RecordA,P.Care,O.Read,-1).reason==D.Reason.Invalid,"Invalid time");
            Check(D.Describe(A.Ai,R.RecordA,P.Care,O.Read,25).EndsWith("|Deny|NoIndependentAuthority",StringComparison.Ordinal),"Fixture summary");
            Check(!D.NetworkEnabled&&!D.ClinicalRelease,"Release gates");
            Debug.Log("VITQRA synthetic community policy checks passed.");
        }
        private static void Check(bool ok,string label){if(!ok)throw new InvalidOperationException(label);}
    }
}
