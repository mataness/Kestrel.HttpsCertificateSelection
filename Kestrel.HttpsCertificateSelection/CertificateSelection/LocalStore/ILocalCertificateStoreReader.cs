using System;
using System.Security.Cryptography.X509Certificates;

namespace Kestrel.HttpsCertificateSelection.CertificateSelection.LocalStore
{
    /// <summary>
    /// Reads certificates from a local certificate store
    /// </summary>
    public interface ILocalCertificateStoreReader : IDisposable
    {
        /// <summary>
        /// Gets certificates from current store
        /// </summary>
        /// <returns>collection of certificates</returns>
        X509Certificate2Collection Certificates { get; }

        /// <summary>
        /// gets current store location
        /// </summary>
        /// <returns>Current Store Location</returns>
        StoreLocation Location { get; }

        /// <summary>
        /// Opens the certificate store
        /// </summary>
        /// <param name="flags">Permission flags to open store with</param>
        void Open(OpenFlags flags);

        /// <summary>
        /// Closes current certificate store
        /// </summary>
        void Close();
    }
}
