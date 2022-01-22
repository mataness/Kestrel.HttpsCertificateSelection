using System;
using System.Security.Cryptography.X509Certificates;
using Azure.Security.KeyVault.Secrets;

namespace Kestrel.HttpsCertificateSelection.AzureKeyVault
{
    /// <summary>
    /// Extension methods for <see cref="KeyVaultSecret"/>
    /// </summary>
    public static class KeyVaultSecretExtensions
    {
        /// <summary>
        /// Converts the secret value to <see cref="X509Certificate2"/> by decoding the secret string value from base-64.
        /// </summary>
        /// <param name="keyVaultSecret">The KeyVault secret</param>
        /// <returns>The <see cref="X509Certificate2"/> converted from the secret value</returns>
        public static X509Certificate2 ToCertificate(this KeyVaultSecret keyVaultSecret)
        {
            try
            {
                _ = keyVaultSecret ?? throw new ArgumentNullException(nameof(keyVaultSecret));

                var certificateContent = Convert.FromBase64String(keyVaultSecret.Value);

                return new X509Certificate2(certificateContent);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("The KeyVault secret does not represent a certificate, see inner error for details", ex);
            }
        }
    }
}
