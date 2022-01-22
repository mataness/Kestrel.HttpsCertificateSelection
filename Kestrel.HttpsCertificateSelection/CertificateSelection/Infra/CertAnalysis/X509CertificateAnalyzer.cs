using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Kestrel.HttpsCertificateSelection.CertificateSelection.Infra.CertAnalysis
{
    /// <summary>
    /// Implements <see cref="IX509CertificateAnalyzer"/>.
    /// Implements <see cref="IsAllowedForServerAuthentication"/> according to Kestrel requirements
    /// See more info here https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.server.kestrel.https.httpsconnectionadapteroptions.servercertificate?view=aspnetcore-6.0#microsoft-aspnetcore-server-kestrel-https-httpsconnectionadapteroptions-servercertificate
    /// </summary>
    public class X509CertificateAnalyzer : IX509CertificateAnalyzer
    {
        public bool IsAllowedForServerAuthentication(X509Certificate2 certificate)
        {
            var ekuExtensions = certificate.Extensions.OfType<X509EnhancedKeyUsageExtension>().ToArray();

            if (!certificate.HasPrivateKey)
            {
                return false;
            }

            if (!ekuExtensions.Any())
            {
                return true;
            }

            // Cert is allowed for server authentication if it has that EKU extension https://oidref.com/1.3.6.1.5.5.7.3.1
            return ekuExtensions
                .Any(extension => extension
                    .EnhancedKeyUsages
                    .OfType<Oid>().Any(oid => oid?.Value == "1.3.6.1.5.5.7.3.1"));
        }

        public bool IsValid(X509Certificate2 certificate)
            => certificate.Verify();
    }
}
