using System;
using System.Security.Cryptography.X509Certificates;

namespace Kestrel.HttpsCertificateSelection.CertificateSelection.Events
{
    /// <summary>
    /// Event args for certificate update events fired by <see cref="IServerCertificateSelector.OnUpdate"/>
    /// </summary>
    public class CertificateUpdateEventArgs : EventArgs
    {
        public CertificateUpdateEventArgs(X509Certificate2 certificate)
        {
            Certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
        }

        /// <summary>
        /// Gets the new certificate that was found and will be used from now on as the server certificate
        /// </summary>
        public X509Certificate2 Certificate { get; }
    }
}
