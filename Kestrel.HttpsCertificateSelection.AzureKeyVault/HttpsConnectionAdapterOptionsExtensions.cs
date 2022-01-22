using System;
using Azure.Security.KeyVault.Secrets;
using Kestrel.HttpsCertificateSelection.CertificateSelection.Infra.CertAnalysis;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;

namespace Kestrel.HttpsCertificateSelection.AzureKeyVault
{
    /// <summary>
    /// Extension methods for <see cref="HttpsConnectionAdapterOptions"/>
    /// </summary>
    public static class HttpsConnectionAdapterOptionsExtensions
    {
        /// <summary>
        /// Configures server (TLS) certificate selection by polling a KeyVault and returning the latest certificate.
        /// This configuration is recommended for cases where the server certificate is stored in KeyVault and the secret has an auto-rotation policy, so the implementation
        /// will always pull the last one and bind it to the Kestrel HTTPS pipeline.
        /// </summary>
        /// <param name="httpsOptions">The HTTPS options</param>
        /// <param name="listenOptions">The listen options (the parent of <see cref="HttpsConnectionAdapterOptions"/>)</param>
        /// <param name="configOptions">The configuration options</param>
        public static void ConfigureKeyVaultServerCertificateSelection(
            this HttpsConnectionAdapterOptions httpsOptions,
            ListenOptions listenOptions,
            Action<ServerKeyVaultCertificateConfigurationOptions> configOptions)
        {
            httpsOptions.ConfigureServerCertificateSelection(listenOptions, serverConfigOptions =>
            {
                var options = new ServerKeyVaultCertificateConfigurationOptions();
                configOptions(options);

                var serverCertificateProvider = new KeyVaultServerCertificateSource(options.SecretName, new SecretClient(options.KeyVaultUrl, options.Credentials), new X509CertificateAnalyzer());
                serverConfigOptions.ValidCertificatesOnly = options.ValidCertificatesOnly;
                serverConfigOptions.PollingInterval = options.PollingInterval;
                serverConfigOptions.SelectorConfigureOptions = options.SelectorConfigureOptions;
                serverConfigOptions.ServerCertificateSource = serverCertificateProvider;
            });
        }
    }

}
