using System;

namespace Kestrel.HttpsCertificateSelection.CertificateSelection.Configuration
{
    /// <summary>
    /// Base class for configurations used in <see cref="HttpsConnectionAdapterOptionsExtensions"/>
    /// </summary>
    public abstract class ServerCertificateConfigurationOptionsBase
    {
        public static TimeSpan DefaultPollingInterval = TimeSpan.FromMinutes(5);

        /// <summary>
        /// The default configuration, contains default value only for non mandatory properties
        /// </summary>
        public static ServerCertificateConfigurationOptions Default = new ServerCertificateConfigurationOptions()
        {
            ValidCertificatesOnly = true,
            PollingInterval = DefaultPollingInterval,
        };

        /// <summary>
        /// The interval of polling for new certificate versions by the <see cref="IServerCertificateSelector"/>.
        /// The value should be determined according to the CPU / Mem / Network resources that is required to poll for a new certificate version.
        /// For example - if the certificate is fetched by making an API call, its recommended to set a reasonable interval so you won't hit throttling limits
        /// by the consumed API.
        /// Initialized by default with <see cref="DefaultPollingInterval"/>
        /// </summary>
        public TimeSpan PollingInterval { get; set; } = Default?.PollingInterval ?? default;

        /// <summary>
        /// Optional - an action that is invoked after the HTTPS pipeline is configured and the <see cref="IServerCertificateSelector"/> was instantiated.
        /// Can be used to register for certificate events
        /// </summary>
        public Action<IServerCertificateSelector> SelectorConfigureOptions { get; set; }

        /// <summary>
        /// A value indicating whether only valid certificates should be used and fetched.
        /// A certificate is considered valid if it passed the certificate validation chain.
        /// This value should be set to true in production environment and can be set to false for testing scenarios.
        /// </summary>
        public bool ValidCertificatesOnly { get; set; } = Default?.ValidCertificatesOnly ?? default;
    }
}
