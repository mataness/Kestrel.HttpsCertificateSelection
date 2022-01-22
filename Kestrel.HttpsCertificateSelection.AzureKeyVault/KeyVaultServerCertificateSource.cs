using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Azure;
using Azure.Security.KeyVault.Secrets;
using Kestrel.HttpsCertificateSelection.CertificateSelection;
using Kestrel.HttpsCertificateSelection.CertificateSelection.Infra.CertAnalysis;

namespace Kestrel.HttpsCertificateSelection.AzureKeyVault
{
    public class KeyVaultServerCertificateSource : IServerCertificateSource
    {
        public KeyVaultServerCertificateSource(string secretName, SecretClient secretClient, IX509CertificateAnalyzer certificateAnalyzer)
        {
            SecretName = secretName ?? throw new ArgumentNullException(nameof(secretName));
            _secretClient = secretClient ?? throw new ArgumentNullException(nameof(secretClient));
            _certificateAnalyzer = certificateAnalyzer ?? throw new ArgumentNullException(nameof(certificateAnalyzer));

            KeyVaultUrl = secretClient.VaultUri;
            Source = $"KeyVault:{KeyVaultUrl};Secret={SecretName}";
        }

        public Uri KeyVaultUrl { get; }

        public string SecretName { get; }

        public string Source { get; }

        public async Task<X509Certificate2> GetLatestCertificateAsync(bool validOnly)
        {
            KeyVaultSecret secret;

            try
            {
                secret = await _secretClient.GetSecretAsync(SecretName);
            }
            catch (RequestFailedException ex)
            {
                throw new ApplicationException("Failed to fetch the server certificate from KeyVault, see inner error for details", ex);
            }

            var secretAsCertificate = secret.ToCertificate();

            var validationFunc = validOnly ? new Func<bool>(() => _certificateAnalyzer.IsValidForServerAuthentication(secretAsCertificate)) : () => _certificateAnalyzer.IsAllowedForServerAuthentication((secretAsCertificate));

            if (validationFunc())
            {
                return secretAsCertificate;
            }

            throw new ApplicationException("The latest KeyVault certificate is not valid for server authentication. " +
                                           $"SecretVersion={secret.Properties.Version}, " +
                                           $"IsValid={_certificateAnalyzer.IsValid(secretAsCertificate)}, " +
                                           $"IsAllowedForServerAuth={_certificateAnalyzer.IsAllowedForServerAuthentication(secretAsCertificate)}");

        }

        private readonly IX509CertificateAnalyzer _certificateAnalyzer;
        private readonly SecretClient _secretClient;
    }
}
