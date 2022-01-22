# Kestrel.HttpsCertificateSelection

Integrate automatic server SSL/TLS certificate rotation to your Web API server!
Fetches the server certificate from a given certificate source (such as machine local certificate store or Azure KeyVault) and periodically queries that certificate source for new certificate versions.
When a new certificate is detected, the library will bind the new certificate version to your server HTTPS pipline, so when your server SSL/TLS certificate is renewed or replaced, the new ceritificate will be picked up automatically by the library.

### Main features:
* Automatic server SSL/TLS certificate rotation
* Easy to configure, integrate and run locally
* Exposes events (callback registrations) to monitor the certificate rotation flow and trigger alarms when it fails
* Thread safe
* Currently supported certificate sources
  * Local certificate store
  * Azure KeyVault
* Easy to extend for any certificate source.

#### When your server SSL/TLS certificate is renewed or replaced, there is no need to restart or redeploy your Web API service!

#### Easy to configure and integrate
The library integrates with Kestrel configuration pipeline and allows you to easily define your server certificate source and the library settings

<pre><code>Host.CreateDefaultBuilder(args)
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
</code></pre>

#### Current built-in supported server certificate sources:
* Local certificate store (Machine / CurrentUser) ![NuGet](https://img.shields.io/nuget/v/Kestrel.HttpsCertificateSelection)
* Azure KeyVault ![NuGet](https://img.shields.io/nuget/v/Kestrel.HttpsCertificateSelection.AzureKeyVault)
* AWS Secret Manager (WIP)
