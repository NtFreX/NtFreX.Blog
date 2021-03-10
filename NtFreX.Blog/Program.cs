using App.Metrics;
using App.Metrics.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NtFreX.Blog.Data;
using NtFreX.Blog.Web;
using NtFreX.ConfigFlow.DotNet;
using NtFreX.Logger.DotNet;
using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace NtFreX.Blog
{
    public class Program
    {
        public const string ClientId = "NtFreX.Blog-064d997e-54ee-4dc1-854c-3c742d2fe54e";

        public static async Task Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var configManager = new ConfigManager(ClientId, Environment.GetEnvironmentVariable("NtFreXBlogClientSecret"));
            var configLoader = await ConfigLoader.LoadAsync(configManager, ConfigNames.MongoDbConnectionString, ConfigNames.MonitoringDatabase, ConfigNames.BlogDatabase);
            var certPassword = await configManager.GetAsync("NtFreX.Blog.CertificatePassword");
            var loggerClientSecret = await configManager.GetAsync("NtFreX.Blog.LoggerClientSecret");
            var loggerPath = await configManager.GetAsync("NtFreX.Logger.Path");
            var reddisConnectionString = await configManager.GetAsync("NtFreX.Blog.RedisConnectionString");

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .Build();

            CreateHostBuilder(args, config, reddisConnectionString, environment == "Development", environment, loggerPath, loggerClientSecret, certPassword, configManager, configLoader).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, IConfigurationRoot configuration, string reddisConnectionString, bool isDevelopment, string environment, string loggerPath, string loggerClientSecret, string certPassword, ConfigManager configManager, ConfigLoader configLoader) =>
             Host.CreateDefaultBuilder(args)
                 .ConfigureLogging(logging =>
                 {
                     logging
                        .AddConsole()
                        .AddNtFreX(ClientId, loggerClientSecret, loggerPath, environment);
                 })
                .ConfigureMetricsWithDefaults(builder =>
                {
                    var flushInterval = TimeSpan.FromMinutes(1);
                    builder.Report.ToDatabase(new Database(configLoader), flushInterval);
                    builder.Report.ToConsole(flushInterval);
                })
                .UseMetrics()
                .UseMetricsWebTracking()
                .ConfigureWebHostDefaults(webHost =>
                {
                    webHost
                        .ConfigureServices(services => {
                            services.Add(ServiceDescriptor.Singleton(configManager));
                            services.Add(ServiceDescriptor.Singleton(configLoader));
                        })
                        .UseConfiguration(configuration)
                        .PreferHostingUrls(isDevelopment)
                        .UseKestrel(options =>
                        {
                            var pwd = configuration["Listeners:Certificate:Password"];
                            var cert = new X509Certificate2(configuration["Listeners:Certificate:Path"], string.IsNullOrEmpty(pwd) ? certPassword : pwd);
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
                        .UseStartup(x => new Startup(x.Configuration, reddisConnectionString));
                });
    }
}
