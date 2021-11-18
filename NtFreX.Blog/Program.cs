using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NtFreX.Blog.Configuration;
using NtFreX.ConfigFlow.DotNet;
using OpenTelemetry.Logs;
using System;
using System.Net;
using System.Net.Security;
using System.Threading.Tasks;

namespace NtFreX.Blog
{
    public class Program
    {
        private static IConfigProvider GetConfigProvider()
        {
            return BlogConfiguration.ConfigProvider switch
            {
                ConfigurationProviderType.Web => new WebConfigProvider(Constants.ClientId, System.Environment.GetEnvironmentVariable(EnvironmentVariables.WebConfigProviderSecret)),
                ConfigurationProviderType.MySql => new MySqlConfigRepository(System.Environment.GetEnvironmentVariable(EnvironmentVariables.MySqlConfigUser), System.Environment.GetEnvironmentVariable(EnvironmentVariables.MySqlConfigPw), System.Environment.GetEnvironmentVariable(EnvironmentVariables.MySqlConfigServer)),
                ConfigurationProviderType.Environment => new EnvironmentConfigProvider(),
                _ => throw new ArgumentException($"Unkonwn ConfigProviderType '{BlogConfiguration.ConfigProvider}' was given")
            };
        }

        public static async Task<(IConfigurationRoot Configuration, IConfigProvider ConfigProvider, ConfigPreloader ConfigLoader, string ReddisConnectionString)> InitializeAppAsync()
        {
            IConfigProvider configProvider = GetConfigProvider();
            var configLoader = await ConfigPreloader.LoadAsync(configProvider, ConfigNames.MongoDbConnectionString, ConfigNames.MySqlDbConnectionString, ConfigNames.BlogDatabaseName, ConfigNames.JwtSecret, ConfigNames.AdminUsername, ConfigNames.AdminPassword);
            var reddisConnectionString = await configProvider.GetAsync(ConfigNames.RedisConnectionString);

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{Configuration.Environment.AspNetCoreEnvironment}.json", optional: true)
                .Build();

            return (config, configProvider, configLoader, reddisConnectionString);
        }

        public static IWebHostBuilder ConfigureWebHostBuilder(IWebHostBuilder webHostBuilder, IConfigurationRoot configuration, IConfigProvider configProvider, ConfigPreloader configLoader, string reddisConnectionString)
        {
            return webHostBuilder
                .ConfigureServices(services =>
                {
                    services.Add(ServiceDescriptor.Singleton(configProvider));
                    services.Add(ServiceDescriptor.Singleton(configLoader));
                })
                .UseConfiguration(configuration)
                .UseStartup(x => new Startup(x.Configuration, reddisConnectionString))
                .UseUrls($"http://*:{BlogConfiguration.HttpPort};https://*:{BlogConfiguration.HttpsPort}");
        }

        public static async Task Main(string[] args)
        {
            var app = await InitializeAppAsync();
            var host = CreateHost(args, app.Configuration, app.ConfigProvider, app.ConfigLoader, app.ReddisConnectionString);
            await host.RunAsync();
        }

        public static IHost CreateHost(string[] args, IConfigurationRoot configuration, IConfigProvider configProvider, ConfigPreloader configLoader, string reddisConnectionString)
        {
            ServerCertificateSelector.Instance.SetConfigProvider(configProvider);

            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging => 
                {
                    logging.ClearProviders();
                    logging.AddLambdaLogger(configuration.GetSection("Logging").Get<LambdaLoggerOptions>());
                    logging.AddOpenTelemetry(options =>
                    {
                        if (!string.IsNullOrEmpty(BlogConfiguration.OtlpLogExporterPath))
                        {
                            options.AddOtlpExporter(options => options.Endpoint = new Uri(BlogConfiguration.OtlpLogExporterPath));
                        }
                    });
                })
                .ConfigureWebHostDefaults(webHost =>
                {
                    ConfigureWebHostBuilder(webHost, configuration, configProvider, configLoader, reddisConnectionString)
                        .UseKestrel(options =>
                        {
                            options.Listen(IPAddress.Any, BlogConfiguration.HttpPort);
                            options.Listen(IPAddress.Any, BlogConfiguration.HttpsPort, listenOptions =>
                            {
                                listenOptions.UseHttps(new ServerOptionsSelectionCallback(async (stream, cientHelloInfo, state, cancelationToken) => new SslServerAuthenticationOptions
                                {
                                    ServerCertificate = await ServerCertificateSelector.Instance.GetCertificateAsync()
                                }), null);
                            });
                        });
                }).Build();
        }
    }
}
