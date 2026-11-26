using System;
using UnityEditor;
using UnityEngine;
using M=Vitqra.VitqraEphemeralTrust;
namespace Vitqra
{
    public static class VitqraEphemeralTrustChecks
    {
        [MenuItem("VITQRA/Run Ephemeral Digital Trust Checks")]
        public static void Run()
        {
            var x=M.Create();var y=M.Create();var clear=M.Revocation.ClearFixture;
            Check(x.Check(25,clear)==M.Result.VerifiedFixtureOnly,"Original");
            Check(x.Check(25,clear,alterDocument:true)==M.Result.SignatureInvalid,"Document changed");
            Check(x.Check(25,clear,alterSignature:true)==M.Result.SignatureInvalid,"Signature changed");
            Check(x.Check(25,clear,other:y)==M.Result.UntrustedFixture,"Other certificate");
            Check(x.Check(25,clear,wrongRevision:true)==M.Result.ScopeMismatch,"Wrong revision");
            Check(x.Check(24,clear)==M.Result.NotYetValid,"Before signing");
            Check(x.Check(25,M.Revocation.Unknown)==M.Result.RevocationUnknown,"Unknown status");
            Check(x.Check(25,M.Revocation.RevokedFixture)==M.Result.Revoked,"Revoked");
            Check(x.Check(59,clear)==M.Result.VerifiedFixtureOnly,"Before revocation");
            Check(x.Check(60,clear)==M.Result.Revoked,"Fixed revocation");
            Check(x.Check(90,clear)==M.Result.Expired,"Expiry");
            Check(x.Check(-1,clear)==M.Result.Invalid,"Invalid day");
            Check(!x.ClinicalRelease&&!x.ScientificApproval&&!x.RealCertificateTrusted,"Release blocked");
            Debug.Log("VITQRA ephemeral fixture checks passed.");
        }
        private static void Check(bool ok,string label){if(!ok)throw new InvalidOperationException(label);}
    }
}
