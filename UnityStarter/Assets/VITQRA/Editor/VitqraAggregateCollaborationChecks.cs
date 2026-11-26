using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using D=Vitqra.VitqraCommunityPolicy;
using M=Vitqra.VitqraAggregateCollaboration;

namespace Vitqra
{
    public static class VitqraAggregateCollaborationChecks
    {
        [MenuItem("VITQRA/Run Synthetic Aggregate Checks")]
        public static void Run()
        {
            int checkedCount=0,allowed=0;
            for(int a=0;a<12;a++)for(int r=0;r<13;r++)for(int p=0;p<6;p++)for(int day=0;day<=180;day++)
            {
                bool expected=r==4&&((a==4&&p==1&&day>=5&&day<65)||(a==5&&p==2&&day>=15&&day<60));
                var result=M.Open((D.Actor)a,(D.Resource)r,(D.Purpose)p,day);
                Check((result.state==M.State.Visible)==expected,"Resource purpose and time");
                if(expected)
                {
                    allowed++;
                    Check(result.rows.Count==4&&result.rows.Count(x=>x.state==M.State.Visible)==2,"Fixed release");
                    Check(result.rows.Where(x=>x.state==M.State.Withheld).All(x=>!x.denominator.HasValue&&x.cells.Count==0),"No suppressed payload");
                    Check(result.digest!=null&&result.digest.Length==64,"Released digest");
                }
                else Check(result.rows.Count==0&&result.digest==null,"No denied payload");
                checkedCount++;
            }
            Check(checkedCount==169416&&allowed==105,"Reference counts");
            var a1=M.Open(D.Actor.Researcher,D.Resource.AggregateA,D.Purpose.Research,25);
            var a2=M.Open(D.Actor.Industry,D.Resource.AggregateA,D.Purpose.Collaboration,25);
            Check(a1.digest==a2.digest&&a1.rows[0].denominator==60&&a1.rows[0].cells[0].count==36&&a1.rows[1].cells[1].count==10,"Shared fixed release");
            Check(M.BoundaryChecks(),"Minimum group, cell and complement");
            Check(M.Open((D.Actor)999,D.Resource.AggregateA,D.Purpose.Research,25).state==M.State.Denied,"Invalid actor");
            Check(M.Open(D.Actor.Industry,D.Resource.AggregateA,D.Purpose.Collaboration,-1).state==M.State.Denied,"Invalid day");
            Check(!M.ClinicalRelease&&!M.ExportEnabled&&!M.NetworkEnabled,"Closed gates");
            Debug.Log("VITQRA synthetic aggregate checks passed.");
        }
        private static void Check(bool ok,string name){if(!ok)throw new InvalidOperationException(name);}
    }
}
