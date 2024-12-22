using System;

namespace SecuredHelloWorldApi
{
    public class KeyVaultServerCertificateConfiguration
    {
        public Uri KeyVaultUrl { get; set; }

        public string SecretName { get; set; }
    }
}
