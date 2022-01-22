using System;
using FluentAssertions;
using Kestrel.HttpsCertificateSelection;
using Kestrel.HttpsCertificateSelection.CertificateSelection;
using Kestrel.HttpsCertificateSelection.CertificateSelection.Events;
using Kestrel.HttpsCertificateSelection.TestUtils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Kestrel.HttpsCertificateSelectionTests
{
    /// <summary>
    /// Integration tests
    /// </summary>
    [TestClass]
    public class IntegrationTests
    {
        /// <summary>
        /// Validates that the Kestrel host is configured with HTTPS as expected
        /// </summary>
        [TestMethod]
        public void Validate_integration_with_kestrel()
        {
            var certProviderMock = new Mock<IServerCertificateSource>();
            var cert = CertificateGenerator.GenerateX509Certificate("SomCN", allowedForServerAuth: true);

            certProviderMock
                .Setup(m => m.GetLatestCertificateAsync(It.IsAny<bool>()))
                .ReturnsAsync(cert);

            CertificateUpdateEventArgs certUpdateEventArgs = null;

            var host = new WebHostBuilder().UseKestrel(options =>
            {
                options.ListenAnyIP(443, options =>
                {
                    options.UseHttps(httpsOptions =>
                    {
                        httpsOptions.ConfigureServerCertificateSelection(
                            options,
                            certificateOptions =>
                            {
                                certificateOptions.ServerCertificateSource = certProviderMock.Object;

                                // valid only must be false because test certs are not really signed
                                certificateOptions.ValidCertificatesOnly = false;
                                certificateOptions.PollingInterval = TimeSpan.FromSeconds(10);
                                certificateOptions.SelectorConfigureOptions = selector => selector.OnUpdate+= (sender, args) => certUpdateEventArgs = args;
                            });
                    });
                });
            })
                .UseStartup<ConfigureService>()
                .ConfigureLogging(o => o.AddDebug().SetMinimumLevel(LogLevel.Debug)).Build();

            host.StartAsync().Wait();

            certUpdateEventArgs.Should().NotBeNull();
            certUpdateEventArgs.Certificate.Should().Be(cert);
        }

        private class ConfigureService
        {
            public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                app.Run(async (context) =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            }
        }
    }
}
