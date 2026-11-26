"""Independent local reference test; requires open-source cryptography.
Keys/certificates/signatures exist only in process memory; output is counts only.
"""
from datetime import datetime, timedelta, timezone
import hashlib
import json
from cryptography import x509
from cryptography.x509.oid import NameOID
from cryptography.hazmat.primitives import hashes, serialization
from cryptography.hazmat.primitives.asymmetric import rsa, padding
from cryptography.exceptions import InvalidSignature

EPOCH = datetime(2026, 1, 1, tzinfo=timezone.utc)
PAYLOAD = b'VITQRA synthetic-only detached review draft\nrevision=2\n'

def generate():
    key = rsa.generate_private_key(public_exponent=65537, key_size=2048)
    name = x509.Name([x509.NameAttribute(NameOID.COMMON_NAME, 'VITQRA fictional test signer')])
    cert = (x509.CertificateBuilder().subject_name(name).issuer_name(name)
            .public_key(key.public_key()).serial_number(x509.random_serial_number())
            .not_valid_before(EPOCH).not_valid_after(EPOCH + timedelta(days=90))
            .add_extension(x509.BasicConstraints(ca=False, path_length=None), critical=True)
            .sign(key, hashes.SHA256()))
    signature = key.sign(PAYLOAD, padding.PKCS1v15(), hashes.SHA256())
    der = cert.public_bytes(serialization.Encoding.DER)
    return der, signature

def verify(der, signature, payload, pin, revision_ok, day, status):
    if day < 0 or day > 180 or status not in ('unknown', 'clear', 'revoked'):
        return 'Invalid'
    if hashlib.sha256(der).digest() != pin:
        return 'UntrustedFixture'
    cert = x509.load_der_x509_certificate(der)
    try:
        cert.public_key().verify(signature, payload, padding.PKCS1v15(), hashes.SHA256())
    except InvalidSignature:
        return 'SignatureInvalid'
    if payload != PAYLOAD or not revision_ok:
        return 'ScopeMismatch'
    now = EPOCH + timedelta(days=day)
    if day < 25 or now < cert.not_valid_before_utc:
        return 'NotYetValid'
    if now >= cert.not_valid_after_utc:
        return 'Expired'
    if status == 'unknown':
        return 'RevocationUnknown'
    if status == 'revoked' or day >= 60:
        return 'Revoked'
    return 'VerifiedFixtureOnly'

def main():
    der, sig = generate()
    other, other_sig = generate()
    pin = hashlib.sha256(der).digest()
    cert = x509.load_der_x509_certificate(der)
    cert.public_key().verify(cert.signature, cert.tbs_certificate_bytes,
                             padding.PKCS1v15(), hashes.SHA256())
    cases = [
        (der, sig, PAYLOAD, True, 25, 'clear', 'VerifiedFixtureOnly'),
        (der, sig, PAYLOAD + b'changed', True, 25, 'clear', 'SignatureInvalid'),
        (der, bytes([sig[0] ^ 1])+sig[1:], PAYLOAD, True, 25, 'clear', 'SignatureInvalid'),
        (other, other_sig, PAYLOAD, True, 25, 'clear', 'UntrustedFixture'),
        (der, sig, PAYLOAD, False, 25, 'clear', 'ScopeMismatch'),
        (der, sig, PAYLOAD, True, 24, 'clear', 'NotYetValid'),
        (der, sig, PAYLOAD, True, 25, 'unknown', 'RevocationUnknown'),
        (der, sig, PAYLOAD, True, 25, 'revoked', 'Revoked'),
        (der, sig, PAYLOAD, True, 59, 'clear', 'VerifiedFixtureOnly'),
        (der, sig, PAYLOAD, True, 60, 'clear', 'Revoked'),
        (der, sig, PAYLOAD, True, 90, 'clear', 'Expired'),
        (der, sig, PAYLOAD, True, -1, 'clear', 'Invalid'),
    ]
    for cert_der, signature, payload, revision, day, status, expected in cases:
        actual = verify(cert_der, signature, payload, pin, revision, day, status)
        assert actual == expected, (actual, expected)
    print(json.dumps({'scope':'synthetic independent reference only', 'cases_passed':len(cases), 'self_signature_verified':True, 'distinct_ephemeral_certificates':der != other, 'unity_executed':False, 'clinical_release':False}, indent=2))

if __name__ == '__main__':
    main()
