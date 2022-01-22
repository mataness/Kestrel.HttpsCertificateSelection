using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Kestrel.HttpsCertificateSelection.CertificateSelection.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kestrel.HttpsCertificateSelection.CertificateSelection
{
    /// <summary>
    /// Implements <see cref="IServerCertificateSelector"/> by periodically polling a given <see cref="IServerCertificateSource"/> and updating the returned
    /// certificate in <see cref="Select"/> according to the latest certificate retrieved by the <see cref="IServerCertificateSource"/>.
    /// The implementation is thread safe
    /// </summary>
    public class PeriodicalPollingServerCertificateSelector : IServerCertificateSelector, IDisposable
    {
        public static readonly TimeSpan MinimumPollingInterval = TimeSpan.FromSeconds(5);

        public PeriodicalPollingServerCertificateSelector(
            IHostApplicationLifetime applicationLifetime,
            IServerCertificateSource serverCertificateSource,
            TimeSpan pollingInterval,
            ILoggerFactory loggerFactory,
            bool validCertificatesOnly)
        {
            ValidCertificatesOnly = validCertificatesOnly;
            _certSource = serverCertificateSource ?? throw new ArgumentNullException(nameof(serverCertificateSource));
            _ = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

            _logger = loggerFactory.CreateLogger(GetType());

            if (applicationLifetime == null)
            {
                throw new ArgumentNullException(nameof(applicationLifetime));
            }

            if (pollingInterval < MinimumPollingInterval)
            {
                throw new ArgumentOutOfRangeException(nameof(pollingInterval), $"Polling interval must be greater or equal to {MinimumPollingInterval}");
            }

            _timer = new Timer(state => GetLatestCertificateAsync(shouldPropagateException: false));

            _pollingInterval = pollingInterval;
            applicationLifetime.ApplicationStarted.Register(Init);
            applicationLifetime.ApplicationStopping.Register(StopTimer);
        }

        public string CertificateSource => _certSource.Source;

        public bool ValidCertificatesOnly { get; }

        private void Init()
        {
            GetLatestCertificateAsync(shouldPropagateException: true).Wait();
        }

        private void ResetTimer()
            => _timer?.Change(_pollingInterval, _pollingInterval);

        private void StopTimer()
            => _timer?.Change(Timeout.Infinite, Timeout.Infinite);

        public event EventHandler<ServerCertificateSelectorFailureEventArgs> OnFailure;
        public event EventHandler<CertificateUpdateEventArgs> OnUpdate;

        public X509Certificate2 Select()
            => _certificate;

        public void Dispose()
            => _timer?.Dispose();

        private async Task GetLatestCertificateAsync(bool shouldPropagateException)
        {
            try
            {
                StopTimer();

                _logger.LogDebug("Attempting to query for a new server certificate");

                var newCert = await _certSource.GetLatestCertificateAsync(ValidCertificatesOnly);

                if (newCert == null)
                {
                    throw new ApplicationException($"Failed to query for server certificate, validate that the certificate exists according to it's search parameters: {_certSource.Source}");
                }

                if (newCert.Equals(_certificate))
                {
                    _logger.LogDebug("Didn't find a new server certificate");

                    return;
                }

                _logger.LogInformation($"Found a new server certificate: {StringifyCert(newCert)}");

                var previousCert = Interlocked.Exchange(ref _certificate, newCert);

                OnUpdate?.Invoke(this, new CertificateUpdateEventArgs(newCert));

                _logger.LogInformation($"Successfully rotated the existing cert. Previous cert: {(previousCert != null ? StringifyCert(previousCert) : "N/A")}");
            }
            catch (Exception ex) when(!shouldPropagateException)
            {
                _logger.LogError("Failed to query for server certificate");

                OnFailure?.Invoke(this, new ServerCertificateSelectorFailureEventArgs(ex, CertificateSource));
            }
            finally
            {
                ResetTimer();
            }
        }

        private string StringifyCert(X509Certificate2 cert)
            => $"{nameof(cert.SubjectName)}:{cert.SubjectName};{nameof(cert.Thumbprint)}:{cert.Thumbprint};{nameof(cert.NotAfter)}:{cert.NotBefore};{nameof(cert.NotAfter)}:{cert.NotAfter}";

        private X509Certificate2 _certificate;
        private readonly IServerCertificateSource _certSource;
        private readonly Timer _timer;
        private readonly TimeSpan _pollingInterval;
        private readonly ILogger _logger;
    }
}
