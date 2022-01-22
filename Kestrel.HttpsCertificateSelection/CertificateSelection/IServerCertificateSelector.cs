using System;
using System.Security.Cryptography.X509Certificates;
using Kestrel.HttpsCertificateSelection.CertificateSelection.Events;
using Microsoft.AspNetCore.Server.Kestrel.Https;

namespace Kestrel.HttpsCertificateSelection.CertificateSelection
{
    /// <summary>
    /// A contract that is being used to get the server certificate.
    /// The implementation should be used in the Kestrel HTTPS pipeline to select the certificate certificate, by configuring
    /// <see cref="HttpsConnectionAdapterOptions.ServerCertificateSelector"/>
    /// </summary>
    public interface IServerCertificateSelector
    {
        /// <summary>
        /// Event that is fired when failing to query the certificate source
        /// </summary>
        event EventHandler<ServerCertificateSelectorFailureEventArgs> OnFailure;

        /// <summary>
        /// Event that is fired when a new certificate was found in the certificate source that is being queried by the implementation
        /// </summary>
        event EventHandler<CertificateUpdateEventArgs> OnUpdate;

        /// <summary>
        /// Gets a description of the certificate source
        /// </summary>
        string CertificateSource { get; }

        /// <summary>
        /// Indicates whether the implementation should only return valid certificate in <see cref="Select"/>.
        /// A certificate is considered valid if it passed the X509 certificate chain validation.
        /// For production scenarios this property must be set to true, but for testing scenarios such as local debugging this property
        /// can be set to false by the user
        /// </summary>
        bool ValidCertificatesOnly { get; }

        /// <summary>
        /// Returns the certificate that should be used by the server for SSL/TLS handshake.
        /// The implementation should query a certificate source and select a certificate that is valid for server side SSL/TLS authentication
        /// </summary>
        /// <returns>The certificate that should be used by the server for SSL/TLS handshake</returns>
        X509Certificate2 Select();
    }
}
