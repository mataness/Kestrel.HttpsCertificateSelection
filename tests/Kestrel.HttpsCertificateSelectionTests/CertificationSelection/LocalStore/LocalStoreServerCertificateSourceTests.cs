using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using Kestrel.HttpsCertificateSelection.CertificateSelection.Infra.CertAnalysis;
using Kestrel.HttpsCertificateSelection.CertificateSelection.LocalStore;
using Kestrel.HttpsCertificateSelection.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TestStack.BDDfy;

namespace Kestrel.HttpsCertificateSelectionTests.CertificationSelection.LocalStore
{
    /// <summary>
    /// Test class for <see cref="LocalStoreServerCertificateSource"/>
    /// </summary>
    [TestClass]
    public class LocalStoreServerCertificateSourceTests
    {
        [TestMethod]
        public void Returns_the_latest_certificate()
        {
            
            this.Given(_ => A_local_certificate_store())
                .And(_ => A_subject_name("MySubject"))
                .When(_ => The_store_contains_a_certificate(_subjectName, DateTime.UtcNow.Subtract(TimeSpan.FromDays(100)), DateTime.UtcNow.Add(TimeSpan.FromDays(100)), true))
                .When(_ => The_store_contains_a_certificate("SomeSubject", DateTime.UtcNow.Subtract(TimeSpan.FromDays(49)), DateTime.UtcNow.Add(TimeSpan.FromDays(100)), true))
                .When(_ => The_store_contains_a_certificate(_subjectName, DateTime.UtcNow.Subtract(TimeSpan.FromDays(80)), DateTime.UtcNow.Add(TimeSpan.FromDays(100)), true))
                .When(_ => The_store_contains_a_certificate("SomeSubject", DateTime.UtcNow.Subtract(TimeSpan.FromDays(80)), DateTime.UtcNow.Add(TimeSpan.FromDays(100)), false))
                .When(_ => The_store_contains_a_certificate(_subjectName, DateTime.UtcNow.Subtract(TimeSpan.FromDays(50)), DateTime.UtcNow.Add(TimeSpan.FromDays(100)), false))
                .When(_ => The_store_contains_a_certificate("SomeSubject", DateTime.UtcNow.Subtract(TimeSpan.FromDays(80)), DateTime.UtcNow.Add(TimeSpan.FromDays(100)), true))
                .When(_ => Getting_the_latest_certificate_for_subject_name(_subjectName))
                .Then(_ => The_returned_certificate_is_in_index(2))
                .BDDfy();
        }

        private void A_local_certificate_store()
        {
            _certificates = new List<X509Certificate2>();
            _reader = new Mock<ILocalCertificateStoreReader>();
            _reader.SetupGet(m => m.Certificates).Returns(() => new X509Certificate2Collection(_certificates.ToArray()));
        }

        private void A_subject_name(string subject)
            => _subjectName = subject;

        private void The_store_contains_a_certificate(string subjectName, DateTime notBefore, DateTime notAfter, bool allowedForServerAuth)
            => _certificates.Add(CertificateGenerator.GenerateX509Certificate(subjectName, notBefore, notAfter, allowedForServerAuth));

        private void Getting_the_latest_certificate_for_subject_name(string subjectName)
            => _returnedCert = new LocalStoreServerCertificateSource(subjectName, _reader.Object, new X509CertificateAnalyzer()).GetLatestCertificateAsync(false).Result;

        private void The_returned_certificate_is_in_index(int idx)
            => _returnedCert.Should().Be(_certificates[idx]);

        private X509Certificate2 _returnedCert;
        private List<X509Certificate2> _certificates;
        private string _subjectName;
        private Mock<ILocalCertificateStoreReader> _reader;
    }
}
