namespace Kestrel.HttpsCertificateSelection.CertificateSelection.Configuration
{
    /// <summary>
    /// The configuration for <see cref="HttpsConnectionAdapterOptionsExtensions.ConfigureServerCertificateSelection"/>
    /// </summary>
    public class ServerCertificateConfigurationOptions : ServerCertificateConfigurationOptionsBase
    {
        /// <summary>
        /// Required - the source of the server certificate.
        /// </summary>
        public IServerCertificateSource ServerCertificateSource { get; set; }
    }
}
