using System;
using Azure.Core;
using Kestrel.HttpsCertificateSelection.CertificateSelection.Configuration;

namespace Kestrel.HttpsCertificateSelection.AzureKeyVault
{
    /// <summary>
    /// The configuration for <see cref="HttpsConnectionAdapterOptionsExtensions.ConfigureKeyVaultServerCertificateSelection"/>
    /// </summary>
    public class ServerKeyVaultCertificateConfigurationOptions : ServerCertificateConfigurationOptionsBase
    {
        /// <summary>
        /// Required - the URL of the KeyVault which contains the secret
        /// </summary>
        public Uri KeyVaultUrl { get; set; }

        /// <summary>
        /// Required - the secret name
        /// </summary>
        public string SecretName { get; set; }

        /// <summary>
        /// Credentials that will be used to authenticate against KeyVault. The given credentials needs to have read permission on <see cref="SecretName"/>
        /// </summary>
        public TokenCredential Credentials { get; set; }
    }
}
