using App.Metrics;
using App.Metrics.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NtFreX.Blog.Data;
using NtFreX.Blog.Web;
using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace NtFreX.Blog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .Build();

            CreateHostBuilder(args, config, environment == "Development").Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, IConfigurationRoot configuration, bool isDevelopment) =>
             Host.CreateDefaultBuilder(args)
                 .ConfigureLogging(logging =>
                 {
                     logging.AddConsole();
                 })
                .ConfigureMetricsWithDefaults(builder =>
                {
                    var flushInterval = TimeSpan.FromMinutes(1);
                    builder.Report.ToDatabase(new Database(configuration), flushInterval);
                    builder.Report.ToConsole(flushInterval);
                })
                .UseMetrics()
                .UseMetricsWebTracking()
                .ConfigureWebHostDefaults(webHost =>
                {
                    webHost
                        .UseConfiguration(configuration)
                        .PreferHostingUrls(isDevelopment)
                        .UseKestrel(options =>
                        {
                            var pwd = configuration["Listeners:Certificate:Password"];
                            var cert = new X509Certificate2(configuration["Listeners:Certificate:Path"], string.IsNullOrEmpty(pwd) ? Environment.GetEnvironmentVariable("NTFREXBLOGCERTPWD") : pwd);
                            options.ConfigureHttpsDefaults(listenOptions =>
                            {
                                listenOptions.ServerCertificate = cert;
                            });
                            options.Listen(IPAddress.Any, int.Parse(configuration["Listeners:Ports:HTTP"]));
                            options.Listen(IPAddress.Any, int.Parse(configuration["Listeners:Ports:HTTPS"]), listenOptions =>
                            {
                                listenOptions.UseHttps(cert);
                            });

                        })
                        .UseStartup<Startup>();
                });
    }
}
