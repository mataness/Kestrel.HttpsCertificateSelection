using Kestrel.HttpsCertificateSelection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Security.Cryptography.X509Certificates;

namespace SecuredHelloWorldApiLocalCertificateCustomStore
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

                    webBuilder.UseKestrel((ctx, kestrelOptions) => kestrelOptions.ListenAnyIP(44332, listenOptions =>
                    {
                        listenOptions.UseHttps(httpsOptions =>
                        {
                            httpsOptions.ConfigureLocalStoreServerCertificateSelection(listenOptions,
                                (localOptions) =>
                                {
                                    localOptions.FindValue = "localhost";
                                    localOptions.Location = StoreLocation.LocalMachine;
                                    localOptions.StoreName = "WebHosting";
                                    localOptions.PollingInterval = TimeSpan.FromMinutes(60);

                                    localOptions.SelectorConfigureOptions = selectorConfig =>
                                    {
                                        selectorConfig.OnUpdate += (caller, cert) =>
                                        {
                                            Console.WriteLine("Cert updated");
                                        };
                                        selectorConfig.OnFailure += (caller, cert) =>
                                        {
                                            Console.WriteLine("Failed to rotate");
                                        };
                                    };
                                });
                        });
                    }));

                });
        }
    }
}
