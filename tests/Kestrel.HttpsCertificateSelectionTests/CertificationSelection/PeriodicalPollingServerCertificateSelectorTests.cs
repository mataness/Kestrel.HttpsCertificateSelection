using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using FluentAssertions;
using Kestrel.HttpsCertificateSelection.CertificateSelection;
using Kestrel.HttpsCertificateSelection.TestUtils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TestStack.BDDfy;

namespace Kestrel.HttpsCertificateSelectionTests.CertificationSelection
{
    /// <summary>
    /// Test class for <see cref="PeriodicalPollingServerCertificateSelector"/>
    /// </summary>
    [TestClass]
    public class PeriodicalPollingServerCertificateSelectorTests
    {
        [TestInitialize]
        public void Initialize()
        {
            _appStarted = new CancellationTokenSource();
            _lifetimeMock = new Mock<IHostApplicationLifetime>();
            _lifetimeMock
                .SetupGet(m => m.ApplicationStarted)
                .Returns(_appStarted.Token);

            _certificateUpdateNotifications = new HashSet<X509Certificate2>();
            _failureEventRaised = default;
            _expectedCertificates = new List<X509Certificate2>();
        }

        [TestMethod]
        public void Fetches_for_the_first_time_when_the_application_has_started()
        {
            this.Given(_ => A_certificate_provider())
                .And(_ => Polling_interval(TimeSpan.FromMinutes(60)))
                .When(_ => Creating_the_certificate_selector())
                .And(_ => Starting_the_application())
                .Then(_ => The_certificate_was_fetched_for_the_first_time_right_after_the_application_has_started())
                .BDDfy();
        }

        [TestMethod]
        public void Notifies_when_a_new_certificate_was_provided()
        {
            this.Given(_ => A_certificate_provider_which_returns_different_certificate_in_two_consecutive_calls())
                .And(_ => Polling_interval(PeriodicalPollingServerCertificateSelector.MinimumPollingInterval))
                .When(_ => Creating_the_certificate_selector())
                .And(_ => Starting_the_application())
                .And(_ => Waiting_for(TimeSpan.FromMilliseconds(100)))
                .Then(_ => Certificate_update_notification_was_raised(1))
                .And(_ => Waiting_for(TimeSpan.FromSeconds(6)))
                .Then(_ => Certificate_update_notification_was_raised(2))
                .BDDfy();
        }

        [TestMethod]
        public void Passes_arguments_to_the_certificate_provider_as_expected()
        {
            bool shouldBeValid = default;

            this.Given(_ => A_certificate_provider())
                .And(_ => Polling_interval(TimeSpan.FromMinutes(60)))
                .And(_ => The_returned_certificates_should_be_valid(shouldBeValid))
                .When(_ => Creating_the_certificate_selector())
                .And(_ => Starting_the_application())
                .Then(_ => The_certificate_provider_was_called_with_the_expected_parameters())
                .WithExamples(new ExampleTable(nameof(shouldBeValid))
                {
                    { true },
                    { false }
                })
                .BDDfy();
        }

        [ExpectedException(typeof(AggregateException))]
        [TestMethod]
        public void Propagates_when_error_occurred_on_the_first_attempt()
        {
            this.Given(_ => A_certificate_provider_which_always_fail())
                .And(_ => Polling_interval(TimeSpan.FromMinutes(60)))
                .When(_ => Creating_the_certificate_selector())
                .And(_ => Starting_the_application())
                .BDDfy();
        }

        [TestMethod]
        public void Raises_event_when_the_first_certificate_fetched_successfully_but_further_attempts_fail()
        {
            this.Given(_ => A_certificate_provider_which_succeed_on_the_first_attempt_and_then_always_fail())
                .And(_ => Polling_interval(PeriodicalPollingServerCertificateSelector.MinimumPollingInterval))
                .When(_ => Creating_the_certificate_selector())
                .And(_ => Starting_the_application())
                .And(_ => Waiting_for(TimeSpan.FromSeconds(6)))
                .Then(_ => Failure_event_notification_was_raised())
                .BDDfy();
        }

        [TestMethod]
        public void Returns_the_new_certificate_when_replaced()
        {
            this.Given(_ => A_certificate_provider_which_returns_different_certificate_in_two_consecutive_calls())
                .And(_ => Polling_interval(PeriodicalPollingServerCertificateSelector.MinimumPollingInterval))
                .When(_ => Creating_the_certificate_selector())
                .And(_ => Starting_the_application())
                .And(_ => Getting_the_certificate())
                .Then(_ => The_first_certificate_has_been_returned())
                .And(_ => Waiting_for(TimeSpan.FromSeconds(6)))
                .And(_ => Getting_the_certificate())
                .Then(_ => The_second_certificate_has_been_returned())
                .BDDfy();
        }

        private void A_certificate_provider_which_succeed_on_the_first_attempt_and_then_always_fail()
        {
            _certProviderMock = new Mock<IServerCertificateSource>();
            _certProviderMock.SetupSequence(m => m.GetLatestCertificateAsync(It.IsAny<bool>()))
                .ReturnsAsync(CertificateGenerator.GenerateX509Certificate("CN=Hello"))
                .ThrowsAsync(new InvalidOperationException("Surprise"));
        }

        private void A_certificate_provider_which_always_fail()
        {
            _certProviderMock = new Mock<IServerCertificateSource>();
            _certProviderMock.Setup(m => m.GetLatestCertificateAsync(It.IsAny<bool>()))
                .ThrowsAsync(new InvalidOperationException("Surprise"));
        }

        private void The_returned_certificates_should_be_valid(bool validCertificatesOnly)
            => _shouldReturnValidCertificatesOnly = validCertificatesOnly;

        private void A_certificate_provider()
        {
            _certProviderMock = new Mock<IServerCertificateSource>();

            _certProviderMock.Setup(m => m.GetLatestCertificateAsync(It.IsAny<bool>()))
                .Callback<bool>(_ => _providerInvocationTimeStamp = DateTime.UtcNow)
                .ReturnsAsync(CertificateGenerator.GenerateX509Certificate("CN=SomeCert"));
        }

        private void Polling_interval(TimeSpan pollingInterval)
            => _pollingInterval = pollingInterval;

        private void A_certificate_provider_which_returns_different_certificate_in_two_consecutive_calls()
        {
            _certProviderMock = new Mock<IServerCertificateSource>();

            _expectedCertificates.Add(CertificateGenerator.GenerateX509Certificate("CN=SomeOtherCert"));
            _expectedCertificates.Add(CertificateGenerator.GenerateX509Certificate("CN=SomeOtherCert2"));

            _certProviderMock.SetupSequence(m => m.GetLatestCertificateAsync(It.IsAny<bool>()))
                .ReturnsAsync(_expectedCertificates[0])
                .ReturnsAsync(_expectedCertificates[1]);
        }

        private void Creating_the_certificate_selector()
        {
            _certificateSelector = new PeriodicalPollingServerCertificateSelector(_lifetimeMock.Object, _certProviderMock.Object, _pollingInterval, new NullLoggerFactory(), _shouldReturnValidCertificatesOnly);

            _certificateSelector.OnUpdate += (sender, args) => _certificateUpdateNotifications.Add(args.Certificate);
            _certificateSelector.OnFailure += (sender, args) => _failureEventRaised = true;
        }

        private void The_first_certificate_has_been_returned()
            => _returnedCertificate.Should().Be(_expectedCertificates[0]);

        private void The_second_certificate_has_been_returned()
            => _returnedCertificate.Should().Be(_expectedCertificates[1]);

        private void Waiting_for(TimeSpan waitTime)
            => Thread.Sleep(waitTime);

        private void Starting_the_application()
            => _appStarted.Cancel();

        private void Getting_the_certificate()
            => _returnedCertificate = _certificateSelector.Select();

        private void The_certificate_was_fetched_for_the_first_time_right_after_the_application_has_started()
            => _providerInvocationTimeStamp.Subtract(DateTime.UtcNow).Should().BeLessOrEqualTo(TimeSpan.FromMilliseconds(200));

        private void The_certificate_provider_was_called_with_the_expected_parameters()
            => _certProviderMock.Verify(m => m.GetLatestCertificateAsync(_shouldReturnValidCertificatesOnly), Times.Once);

        private void Certificate_update_notification_was_raised(int expectedCount)
        {
            _certificateUpdateNotifications.Should().HaveCount(expectedCount);
        }

        private void Failure_event_notification_was_raised()
            => _failureEventRaised.Should().BeTrue();

        private Mock<IServerCertificateSource> _certProviderMock;
        private TimeSpan _pollingInterval;
        private PeriodicalPollingServerCertificateSelector _certificateSelector;
        private HashSet<X509Certificate2> _certificateUpdateNotifications;
        private bool _failureEventRaised;
        private bool _shouldReturnValidCertificatesOnly;
        private Mock<IHostApplicationLifetime> _lifetimeMock;
        private X509Certificate2 _returnedCertificate;
        private CancellationTokenSource _appStarted;
        private DateTime _providerInvocationTimeStamp;
        private List<X509Certificate2> _expectedCertificates;
    }
}
