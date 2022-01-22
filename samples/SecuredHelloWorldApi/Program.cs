using System;
using Azure.Identity;
using Kestrel.HttpsCertificateSelection.AzureKeyVault;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace SecuredHelloWorldApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();

                    webBuilder.UseKestrel((ctx, kestrelOptions) => kestrelOptions.ListenAnyIP(443, listenOptions =>
                    {
                        listenOptions.UseHttps(httpsOptions =>
                        {
                            httpsOptions.ConfigureKeyVaultServerCertificateSelection(
                                listenOptions,
                                configOptions =>
                                {
                                    // Set the authentication method against KeyVault
                                    configOptions.Credentials = new ManagedIdentityCredential();

                                    // Read the KeyVault server certificate configuration from appsettings.json
                                    var keyVaultConfig = ctx.Configuration.GetSection(nameof(KeyVaultServerCertificateConfiguration)).Get<KeyVaultServerCertificateConfiguration>();
                                    
                                    // Initialize the configuration options
                                    configOptions.SecretName = keyVaultConfig.SecretName;
                                    configOptions.KeyVaultUrl = keyVaultConfig.KeyVaultUrl;

                                    // Register to certificate change event, you can use these events to submit metrics and monitor the certificate rotation flow
                                    configOptions.SelectorConfigureOptions = selectorConfig => selectorConfig.OnUpdate += (caller, cert) => Console.WriteLine("Cert updated");
                                    configOptions.SelectorConfigureOptions = selectorConfig => selectorConfig.OnFailure += (caller, cert) => Console.WriteLine("Cert rotation failed!");
                                });
                        });
                    }));

                });
        }
    }
}
