using System;

namespace Kestrel.HttpsCertificateSelection.CertificateSelection.Events
{
    /// <summary>
    /// Event args for certificate update events fired by <see cref="IServerCertificateSelector.OnFailure"/>
    /// </summary>
    public class ServerCertificateSelectorFailureEventArgs : EventArgs
    {
        public ServerCertificateSelectorFailureEventArgs(Exception exception, string certificateSource)
        {
            Exception = exception;
            CertificateSource = certificateSource;
        }

        /// <summary>
        /// Gets the error
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Gets a description of the source of the certificate
        /// </summary>
        public string CertificateSource { get; }
    }
}
