using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using D=Vitqra.VitqraCommunityPolicy;
using A=Vitqra.VitqraCommunityPolicy.Actor;
using R=Vitqra.VitqraCommunityPolicy.Resource;

namespace Vitqra
{
    public static class VitqraPrevisitChecks
    {
        [MenuItem("VITQRA/Run Synthetic Previsit Checks")]
        public static void Run()
        {
            int count=0,allowed=0;
            for(int a=0;a<12;a++)for(int r=0;r<2;r++)for(int t=0;t<=180;t++)for(int c=0;c<=30;c++)
            {
                bool expected=c<30&&c<=t&&((a==r&&t<90)||(a==r+2&&t>=10&&t<(r==0?50:70)));
                var x=VitqraPrevisit.Open((A)a,(R)r,t,c);
                Check((x.summary!=null)==expected,"Access and cutoff");
                if(x.summary!=null)
                {
                    Check(x.summary.observations.All(o=>o.observed<=c&&o.available<=c&&o.id!=10),"No future data");
                    allowed++;
                }
                count++;
            }
            Check(count==134664&&allowed==7150,"Reference totals");
            var s=VitqraPrevisit.Open(A.ClinicianA,R.RecordA,25,25).summary;
            Check(s.observations.Count==8&&s.knownMissing==1&&s.placeholders==2&&s.changeA==4&&s.changeB==-4,"Day 25 summary");
            Check(!s.observations.Any(o=>o.id==8),"Delayed item excluded");
            Check(VitqraPrevisit.Open(A.ClinicianA,R.RecordA,29,29).summary.observations.Count==9,"Delayed item available");
            Check(VitqraPrevisit.Open(A.ClinicianA,R.RecordA,50,25).summary==null,"Revocation cannot be replayed");
            Check(VitqraPrevisit.Open(A.ClinicianB,R.RecordB,70,25).summary==null,"Expiry cannot be replayed");
            Check(VitqraPrevisit.Open(A.ClinicianA,R.RecordB,25,25).summary==null,"Cross patient denied");
            Check(VitqraPrevisit.Open(A.Ai,R.RecordA,25,25).summary==null,"AI denied");
            Check(VitqraPrevisit.Open(A.PatientA,R.StudyA,25,25).summary==null,"Nonclinical resource rejected");
            Check(VitqraPrevisit.Open(A.PatientA,R.RecordA,25,26).summary==null,"Future cutoff");
            Check(s.digest.Length==64&&s.digest==VitqraPrevisit.Open(A.PatientA,R.RecordA,25,25).summary.digest,"Stable scoped summary");
            Check(s.digest!=VitqraPrevisit.Open(A.PatientB,R.RecordB,25,25).summary.digest,"Resource binding");
            Check(!VitqraPrevisit.ClinicalRelease&&!VitqraPrevisit.ExportEnabled&&!D.NetworkEnabled,"Closed gates");
            Debug.Log("VITQRA synthetic previsit checks passed.");
        }
        private static void Check(bool ok,string label){if(!ok)throw new InvalidOperationException(label);}
    }
}
