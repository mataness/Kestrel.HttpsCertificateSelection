using FluentAssertions;
using Kestrel.HttpsCertificateSelection.CertificateSelection.Infra.CertAnalysis;
using Kestrel.HttpsCertificateSelection.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kestrel.HttpsCertificateSelectionTests.CertificationSelection.Infra.CertAnalysis
{
    /// <summary>
    /// Test class for <see cref="X509CertificateAnalyzer"/>
    /// </summary>
    [TestClass]
    public class X509CertificateAnalyzerTests
    {
        [TestMethod]
        public void Returns_true_when_the_certificate_allowed_for_server_auth()
        {
            var cert = CertificateGenerator.GenerateX509Certificate("CN=MyCn", allowedForServerAuth: true);

            new X509CertificateAnalyzer().IsAllowedForServerAuthentication(cert).Should().BeTrue();
        }

        [TestMethod]
        public void Returns_false_when_the_certificate_is_not_allowed_for_server_auth()
        {
            var cert = CertificateGenerator.GenerateX509Certificate("CN=MyCn", allowedForServerAuth: false);

            new X509CertificateAnalyzer().IsAllowedForServerAuthentication(cert).Should().BeFalse();
        }
    }
}
