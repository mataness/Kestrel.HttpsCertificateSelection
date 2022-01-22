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
            string subjectName,
            ILocalCertificateStoreReader localCertificateStoreReader,
            IX509CertificateAnalyzer certificateAnalyzer)
        {
            _localCertificateStoreReader = localCertificateStoreReader ?? throw new ArgumentNullException(nameof(localCertificateStoreReader));
            _certificateAnalyzer = certificateAnalyzer ?? throw new ArgumentNullException(nameof(certificateAnalyzer));
            SubjectName = string.IsNullOrWhiteSpace(subjectName) ? throw new ArgumentNullException() : subjectName;
        }

        public string SubjectName { get; }

        public string Source =>
            "Local Certificate Store: " +
            $"{nameof(_localCertificateStoreReader.Location)}:{_localCertificateStoreReader.Location}, " +
            $"{nameof(SubjectName)}:{SubjectName}";

        public Task<X509Certificate2> GetLatestCertificateAsync(bool validOnly)
        {
            try
            {
                _localCertificateStoreReader.Open(OpenFlags.ReadOnly);

                var now = DateTime.Now;
                var latestCert = _localCertificateStoreReader
                    .Certificates
                    .Find(X509FindType.FindBySubjectName, SubjectName, validOnly: validOnly)
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
