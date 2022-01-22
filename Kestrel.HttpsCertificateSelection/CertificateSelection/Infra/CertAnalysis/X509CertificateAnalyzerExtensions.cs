using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Kestrel.HttpsCertificateSelection.CertificateSelection.Infra.CertAnalysis
{
    /// <summary>
    /// Extension methods for <see cref="IX509CertificateAnalyzer"/>
    /// </summary>
    public static class X509CertificateAnalyzerExtensions
    {
        public static bool IsValidForServerAuthentication(this IX509CertificateAnalyzer analyzer, X509Certificate2 certificate)
        {
            _ = analyzer ?? throw new ArgumentNullException(nameof(analyzer));
            _ = certificate ?? throw new ArgumentNullException(nameof(certificate));

            return analyzer.IsAllowedForServerAuthentication(certificate) && analyzer.IsValid(certificate);
        }
    }
}
