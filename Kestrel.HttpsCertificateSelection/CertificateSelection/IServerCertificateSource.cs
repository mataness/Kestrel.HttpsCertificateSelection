using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Kestrel.HttpsCertificateSelection.CertificateSelection
{
    /// <summary>
    /// A source of server certificate from some source, such as local certificate store, Azure KeyVault, AWS Secrets Manager.
    /// </summary>
    public interface IServerCertificateSource
    {
        /// <summary>
        /// Gets a description for the source of certificate (e.g. Local certificate store)
        /// </summary>
        string Source { get; }

        /// <summary>
        /// Gets the latest certificate that should be used by the server for SSL/TLS handshake
        /// </summary>
        /// <param name="validOnly">When set to true, the implementation should return only a valid certificate that passed the X509 certificate chain validation</param>
        /// <returns></returns>
        Task<X509Certificate2> GetLatestCertificateAsync(bool validOnly);
    }
}
