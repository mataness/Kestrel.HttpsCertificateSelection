using System.Security.Cryptography.X509Certificates;

namespace Kestrel.HttpsCertificateSelection.CertificateSelection.Configuration
{
    /// <summary>
    /// The configuration for <see cref="HttpsConnectionAdapterOptionsExtensions.ConfigureLocalStoreServerCertificateSelection"/>
    /// </summary>
    public class ServerLocalCertificateStoreConfigurationOptions : ServerCertificateConfigurationOptionsBase
    {
        /// <summary>
        /// Required - the subject name of the certificate to fetch from the local certificate store
        /// </summary>
        public string SubjectName { get; set; }

        /// <summary>
        /// Required - the store location, defaults to <see cref="StoreLocation.LocalMachine"/>
        /// </summary>
        public StoreLocation Location { get; set; } = StoreLocation.LocalMachine;
    }
}
