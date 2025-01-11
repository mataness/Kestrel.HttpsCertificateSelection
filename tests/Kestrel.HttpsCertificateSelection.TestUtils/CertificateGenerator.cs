using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace Kestrel.HttpsCertificateSelection.TestUtils
{
    public class CertificateGenerator
    {
        public static X509Certificate2 GenerateX509Certificate(string commonName, DateTime? notBefore = null, DateTime? notAfter = null, bool allowedForServerAuth = false)
        {
            var keyPair = GetAsymmetricCipherKeyPair();

            var certificateGenerator = GetX509V3CertificateGenerator(keyPair, notBefore, notAfter);

            SetSubjectAndIssuer(certificateGenerator, commonName, allowedForServerAuth);

            var bouncyCastleCertificate = GenerateBouncyCastleCertificate(keyPair, certificateGenerator);

            return GenerateX509CertificateWithPrivateKey(keyPair, bouncyCastleCertificate);
        }

        private static void SetSubjectAndIssuer(X509V3CertificateGenerator gen, string commonName, bool allowedForServerAuth = false)
        {
            var attrs = new Dictionary<DerObjectIdentifier, string>
            {
                [X509Name.CN] = commonName
            };

            var ord = new List<DerObjectIdentifier>
            {
                X509Name.CN
            };

            if (!allowedForServerAuth)
            {
                gen.AddExtension(
                    X509Extensions.ExtendedKeyUsage.Id,
                    false,
                    new ExtendedKeyUsage(new[] { KeyPurposeID.id_kp_clientAuth }));
            }

            gen.SetSubjectDN(new X509Name(ord, attrs));
            gen.SetIssuerDN(new X509Name(ord, attrs));
        }

        private static AsymmetricCipherKeyPair GetAsymmetricCipherKeyPair()
        {
            var keyPairGen = new RsaKeyPairGenerator();
            var keyParams = new KeyGenerationParameters(new SecureRandom(new CryptoApiRandomGenerator()), 2048);
            keyPairGen.Init(keyParams);
            var keyPair = keyPairGen.GenerateKeyPair();

            return keyPair;
        }

        private static X509V3CertificateGenerator GetX509V3CertificateGenerator(
            AsymmetricCipherKeyPair keyPair,
            DateTime? notBefore,
            DateTime? notAfter)
        {
            var gen = new X509V3CertificateGenerator();
            gen.SetSerialNumber(BigInteger.ProbablePrime(120, new Random()));
            gen.SetNotAfter(notAfter ?? DateTime.Now.AddDays(1));
            gen.SetNotBefore(notBefore ?? DateTime.Now.AddDays(-1));
            gen.SetPublicKey(keyPair.Public);

            var keyUsage = new KeyUsage(KeyUsage.DigitalSignature | KeyUsage.KeyCertSign);
            gen.AddExtension(X509Extensions.KeyUsage, true, keyUsage);

            gen.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(false));

            var ski = new SubjectKeyIdentifier(SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keyPair.Public));
            gen.AddExtension(X509Extensions.SubjectKeyIdentifier, false, ski);

            return gen;
        }

        private static X509Certificate GenerateBouncyCastleCertificate(AsymmetricCipherKeyPair keyPair, X509V3CertificateGenerator gen)
        {
            var sigFact = new Asn1SignatureFactory("SHA256WithRSA", keyPair.Private);
            var bcCert = gen.Generate(sigFact);

            return bcCert;
        }

        private static X509Certificate2 GenerateX509CertificateWithPrivateKey(AsymmetricCipherKeyPair keyPair, X509Certificate bcCert)
        {
            var privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keyPair.Private);
            var asn1Seq = (Asn1Sequence)Asn1Object.FromByteArray(privateKeyInfo.ParsePrivateKey().GetDerEncoded());
            var rsaPrivateKeyStruct = RsaPrivateKeyStructure.GetInstance(asn1Seq);
            var rsa = DotNetUtilities.ToRSA(rsaPrivateKeyStruct);
            var x509Cert = new X509Certificate2(bcCert.GetEncoded());

            return x509Cert.CopyWithPrivateKey(rsa);
        }
    }
}
