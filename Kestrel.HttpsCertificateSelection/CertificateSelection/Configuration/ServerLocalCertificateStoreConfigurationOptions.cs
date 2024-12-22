using System.Security.Cryptography.X509Certificates;

namespace Kestrel.HttpsCertificateSelection.CertificateSelection.Configuration
{
    /// <summary>
    /// The configuration for <see cref="HttpsConnectionAdapterOptionsExtensions.ConfigureLocalStoreServerCertificateSelection"/>
    /// </summary>
    public class ServerLocalCertificateStoreConfigurationOptions : ServerCertificateConfigurationOptionsBase
    {
        /// <summary>
        /// Required - the find value of the certificate to fetch from the local certificate store. See <see cref="X509Certificate2Collection.Find(X509FindType, object, bool)"/>
        /// </summary>
        public string FindValue { get; set; }

        /// <summary>
        /// The find type, defaults to <see cref="X509FindType.FindBySubjectName"/>. See <see cref="X509FindType"/>
        /// </summary>
        public X509FindType FindType { get; set; } = X509FindType.FindBySubjectName;

        /// <summary>
        /// Required - the store location, defaults to <see cref="StoreLocation.LocalMachine"/>
        /// </summary>
        public StoreLocation Location { get; set; } = StoreLocation.LocalMachine;
    }
}
