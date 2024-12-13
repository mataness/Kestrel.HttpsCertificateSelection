using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Kestrel.HttpsCertificateSelection.CertificateSelection.Infra.CertAnalysis;

namespace Kestrel.HttpsCertificateSelection.CertificateSelection.LocalStore
{
    /// <summary>
    /// Local certificate store source. Gets the latest certificate matching a given subject name.
    /// The implementation selects the latest certificate according to the <see cref="X509Certificate2.NotBefore"/>
    /// </summary>
    public class LocalStoreServerCertificateSource : IServerCertificateSource
    {
        public LocalStoreServerCertificateSource(
            object findValue,
            X509FindType findType,
            ILocalCertificateStoreReader localCertificateStoreReader,
            IX509CertificateAnalyzer certificateAnalyzer)
        {
            _localCertificateStoreReader = localCertificateStoreReader ?? throw new ArgumentNullException(nameof(localCertificateStoreReader));
            _certificateAnalyzer = certificateAnalyzer ?? throw new ArgumentNullException(nameof(certificateAnalyzer));
            FindValue = findValue ?? throw new ArgumentNullException();
            FindType = findType;
        }

        public object FindValue { get; }

        public X509FindType FindType { get; }

        public string Source =>
            "Local Certificate Store: " +
            $"{nameof(_localCertificateStoreReader.Location)}:{_localCertificateStoreReader.Location}, " +
            $"{nameof(FindType)}:{FindType}, " +
            $"{nameof(FindValue)}:{FindValue}";

        public Task<X509Certificate2> GetLatestCertificateAsync(bool validOnly)
        {
            try
            {
                _localCertificateStoreReader.Open(OpenFlags.ReadOnly);

                var now = DateTime.Now;
                var latestCert = _localCertificateStoreReader
                    .Certificates
                    .Find(FindType, FindValue, validOnly: validOnly)
                    .Cast<X509Certificate2>()
                    .Where(cert => _certificateAnalyzer.IsAllowedForServerAuthentication(cert))
                    .OrderByDescending(cert => cert.NotBefore)
                    .FirstOrDefault(cert => cert.NotBefore <= now);

                return Task.FromResult(latestCert);
            }
            finally
            {
                _localCertificateStoreReader.Close();
            }
        }

        private readonly ILocalCertificateStoreReader _localCertificateStoreReader;
        private readonly IX509CertificateAnalyzer _certificateAnalyzer;

    }
}
