using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using D=Vitqra.VitqraCommunityPolicy;
using V=Vitqra.VitqraValidationCenter;

namespace Vitqra
{
    // Editor-only fictional exercise. No production PKI, key storage,
    // real signer, imported patient dossier, trust store or network.
    public sealed class VitqraEphemeralTrust
    {
        public enum Revocation { Unknown, ClearFixture, RevokedFixture }
        public enum Result { Invalid, UntrustedFixture, SignatureInvalid, ScopeMismatch, NotYetValid, Expired, RevocationUnknown, Revoked, VerifiedFixtureOnly, CryptoUnavailable }
        private static readonly DateTimeOffset Epoch=new DateTimeOffset(2026,1,1,0,0,0,TimeSpan.Zero);
        private readonly byte[] certificate,signature;
        private readonly string pin,payload,revision;
        private VitqraEphemeralTrust(byte[] der,byte[] sig,string message,string revision)
        {certificate=der;signature=sig;payload=message;this.revision=revision;pin=Hash(der);}
        private static string Hash(byte[] bytes)
        {using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").ToLowerInvariant();}
        public static VitqraEphemeralTrust Create()
        {
            var c=new V();var actor=D.Actor.Reviewer;var reviewer=V.Reviewer.Domain;
            if(c.Submit(actor,reviewer,c.Open(actor,reviewer).digest,V.Verdict.Revise,V.Rationale.DataIncomplete)!=V.Result.Saved||
                !c.SimulateCorrectedRevision(actor,reviewer)||!c.Advance(25))throw new InvalidOperationException("Fixture revision");
            var r=c.Open(actor,reviewer);
            if(c.Submit(actor,reviewer,r.digest,V.Verdict.Favorable,V.Rationale.FixtureChecklist)!=V.Result.Saved)
                throw new InvalidOperationException("Fixture decision");
            var history=c.History(actor,reviewer);var d=history[history.Length-1];
            string message="synthetic-ephemeral-trust-v1\nresource=DossierA\nrevision="+d.revision.ToString(CultureInfo.InvariantCulture)+
                "\nrevision_sha256="+d.digest+"\nreviewer="+d.reviewer+"\nverdict="+d.verdict+"\nrationale="+d.rationale+
                "\nissued_day="+d.day.ToString(CultureInfo.InvariantCulture)+"\nscope=synthetic_only\n";
            using(var rsa=RSA.Create())
            {
                rsa.KeySize=2048;
                var request=new CertificateRequest("CN=VITQRA fictional test signer",rsa,HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1);
                request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false,false,0,true));
                request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature,true));
                using(var cert=request.CreateSelfSigned(Epoch,Epoch.AddDays(90)))
                {
                    var der=cert.RawData;
                    var sig=rsa.SignData(new UTF8Encoding(false,true).GetBytes(message),HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1);
                    return new VitqraEphemeralTrust(der,sig,message,r.digest);
                }
            }
        }
        // Test actions only, no arbitrary document/certificate import API.
        public Result Check(int day,Revocation status,bool alterDocument=false,bool alterSignature=false,
            bool wrongRevision=false,VitqraEphemeralTrust other=null)
        {
            var der=other==null?certificate:other.certificate;
            var sig=(byte[])(other==null?signature:other.signature).Clone();
            if(alterSignature)sig[0]^=1;
            string message=alterDocument?payload+"changed":payload;
            if(day<0||day>180||!Enum.IsDefined(typeof(Revocation),status))return Result.Invalid;
            try
            {
                if(!string.Equals(Hash(der),pin,StringComparison.Ordinal))return Result.UntrustedFixture;
                using(var cert=new X509Certificate2(der))
                using(var rsa=cert.GetRSAPublicKey())
                {
                    if(rsa==null||rsa.KeySize!=2048)return Result.UntrustedFixture;
                    if(!rsa.VerifyData(new UTF8Encoding(false,true).GetBytes(message),sig,HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1))
                        return Result.SignatureInvalid;
                    if(wrongRevision||!message.Contains("revision_sha256="+revision+"\n"))return Result.ScopeMismatch;
                    var now=Epoch.AddDays(day).UtcDateTime;
                    if(day<25||now<cert.NotBefore.ToUniversalTime())return Result.NotYetValid;
                    if(now>=cert.NotAfter.ToUniversalTime())return Result.Expired;
                    if(status==Revocation.Unknown)return Result.RevocationUnknown;
                    if(status==Revocation.RevokedFixture||day>=60)return Result.Revoked;
                    return Result.VerifiedFixtureOnly;
                }
            }
            catch(CryptographicException){return Result.CryptoUnavailable;}
            catch(NotSupportedException){return Result.CryptoUnavailable;}
        }
        public bool ClinicalRelease=>false;
        public bool ScientificApproval=>false;
        public bool RealCertificateTrusted=>false;
    }
}
