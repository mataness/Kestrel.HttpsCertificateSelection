using System.Security.Cryptography.X509Certificates;

namespace Kestrel.HttpsCertificateSelection.CertificateSelection.LocalStore
{
    /// <summary>
    /// Implements <see cref="ILocalCertificateStoreReader"/>
    /// </summary>
    public class LocalCertificateStoreReader : ILocalCertificateStoreReader
    {
        public LocalCertificateStoreReader(StoreLocation location)
        {
            _certStore = new X509Store(StoreName.My, location);
        }

        /// <inheritdoc/>
        public StoreLocation Location
            => _certStore.Location;

        /// <inheritdoc/>
        public X509Certificate2Collection Certificates
            => _certStore.Certificates;

        /// <inheritdoc/>
        public void Open(OpenFlags flags)
            => _certStore.Open(flags);

        /// <inheritdoc/>
        public void Close()
        {
            _certStore.Close();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _certStore.Dispose();
        }

        /// <summary>
        /// Certificate Store to be accessed
        /// </summary>
        private readonly X509Store _certStore;
    }
}
