using System;
using Kestrel.HttpsCertificateSelection.CertificateSelection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kestrel.HttpsCertificateSelection
{
    /// <summary>
    /// A static factory for <see cref="IServerCertificateSelector"/>
    /// </summary>
    public static class ServerCertificateSelectorFactory
    {
        /// <summary>
        /// Creates a <see cref="PeriodicalPollingServerCertificateSelector"/>
        /// </summary>
        /// <param name="certificateSource">Will provide the certificates</param>
        /// <param name="serviceProvider">Will be used to create the instance</param>
        /// <param name="pollingInterval">The polling interval</param>
        /// <param name="validCertificatesOnly">Value of <see cref="IServerCertificateSelector.ValidCertificatesOnly"/></param>
        /// <returns></returns>
        public static PeriodicalPollingServerCertificateSelector Create(
            IServerCertificateSource certificateSource,
            IServiceProvider serviceProvider,
            TimeSpan pollingInterval,
            bool validCertificatesOnly)
        {
            _ = certificateSource ?? throw new ArgumentNullException(nameof(certificateSource));
            _ = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            var loggerFactory = serviceProvider.GetService(typeof(ILoggerFactory));

            return new PeriodicalPollingServerCertificateSelector(
                (IHostApplicationLifetime)serviceProvider.GetService(typeof(IHostApplicationLifetime)),
                certificateSource,
                pollingInterval,
                (ILoggerFactory)loggerFactory, validCertificatesOnly);
        }
    }
}
