using System;
using Kestrel.HttpsCertificateSelection.CertificateSelection.Configuration;
using Kestrel.HttpsCertificateSelection.CertificateSelection.Infra.CertAnalysis;
using Kestrel.HttpsCertificateSelection.CertificateSelection.LocalStore;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;

namespace Kestrel.HttpsCertificateSelection
{
    /// <summary>
    /// Extension methods for <see cref="HttpsConnectionAdapterOptions"/>
    /// </summary>
    public static class HttpsConnectionAdapterOptionsExtensions
    {
        /// <summary>
        /// Configures server (TLS) certificate selection with the given configuration options
        /// </summary>
        /// <param name="httpsOptions">The HTTPS options</param>
        /// <param name="listenOptions">The listen options (the parent of <see cref="HttpsConnectionAdapterOptions"/>)</param>
        /// <param name="configOptions">The configuration options</param>
        public static void ConfigureServerCertificateSelection(
            this HttpsConnectionAdapterOptions httpsOptions,
            ListenOptions listenOptions,
            Action<ServerCertificateConfigurationOptions> configOptions)
        {
            var configurationOptions = new ServerCertificateConfigurationOptions();
            configOptions(configurationOptions);

            var certificateSelector = ServerCertificateSelectorFactory.Create(configurationOptions.ServerCertificateSource, listenOptions.ApplicationServices, configurationOptions.PollingInterval, configurationOptions.ValidCertificatesOnly);
            httpsOptions.ServerCertificateSelector = (context, s) => certificateSelector.Select();
            configurationOptions.SelectorConfigureOptions?.Invoke(certificateSelector);
        }

        /// <summary>
        /// Configures server (TLS) certificate selection by polling a local certificate store and returning the latest certificate matching the given configuration.
        /// This configuration is recommended for cases where the local certificate store is being updated by some independent process with the latest TLS certificates, so the implementation
        /// will always pull the last one and bind it to the Kestrel HTTPS pipeline
        /// </summary>
        /// <param name="httpsOptions">The HTTPS options</param>
        /// <param name="listenOptions">The listen options (the parent of <see cref="HttpsConnectionAdapterOptions"/>)</param>
        /// <param name="configOptions">The configuration options</param>
        public static void ConfigureLocalStoreServerCertificateSelection(
            this HttpsConnectionAdapterOptions httpsOptions,
            ListenOptions listenOptions,
            Action<ServerLocalCertificateStoreConfigurationOptions> configOptions)
        {
            httpsOptions.ConfigureServerCertificateSelection(listenOptions, serverConfigOptions =>
            {
                var options = new ServerLocalCertificateStoreConfigurationOptions();
                configOptions(options);

                var localCertStoreReader =
                    string.IsNullOrWhiteSpace(options.StoreName)
                        ? new LocalCertificateStoreReader(options.Location)
                        : new LocalCertificateStoreReader(options.StoreName, options.Location);
                var serverCertificateProvider = new LocalStoreServerCertificateSource(options.FindValue, options.FindType, localCertStoreReader, new X509CertificateAnalyzer());
                serverConfigOptions.ValidCertificatesOnly = options.ValidCertificatesOnly;
                serverConfigOptions.PollingInterval = options.PollingInterval;
                serverConfigOptions.SelectorConfigureOptions = options.SelectorConfigureOptions;
                serverConfigOptions.ServerCertificateSource = serverCertificateProvider;
            });
        }
    }

}
