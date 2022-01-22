using System.Security.Cryptography.X509Certificates;

namespace Kestrel.HttpsCertificateSelection.CertificateSelection.Infra.CertAnalysis
{
    /// <summary>
    /// Analyzes <see cref="X509Certificate2"/> certificates
    /// </summary>
    public interface IX509CertificateAnalyzer
    {
        /// <summary>
        /// Checks if the given certificate can be used for server authentication
        /// </summary>
        /// <param name="certificate">The certificate</param>
        /// <returns>True if can be used for server authentication, false otherwise</returns>
        bool IsAllowedForServerAuthentication(X509Certificate2 certificate);

        /// <summary>
        /// Checks if the given certificate is valid and signed by a known authority
        /// </summary>
        /// <param name="certificate">The certificate</param>
        /// <returns>True if valid, false otherwise</returns>
        bool IsValid(X509Certificate2 certificate);
    }
}
