using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Web;
using NtFreX.Blog.Configuration;
using NtFreX.Blog.Logging;
using OpenTelemetry.Logs;
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Security;
using System.Threading.Tasks;

namespace NtFreX.Blog
{
    public class Program
    {
        public static Meter Meter => new Meter(BlogConfiguration.MetricsName);
        public static ActivitySource ActivitySource => new ActivitySource(BlogConfiguration.ActivitySourceName);

        private static IConfigProvider GetConfigProvider()
        {
            return BlogConfiguration.ConfigProvider switch
            {
                ConfigurationProviderType.Web => new WebConfigProvider(Constants.ClientId, System.Environment.GetEnvironmentVariable(EnvironmentVariables.WebConfigProviderSecret), System.Environment.GetEnvironmentVariable(EnvironmentVariables.WebConfigProviderPath)),
                ConfigurationProviderType.MySql => new MySqlConfigProvider(System.Environment.GetEnvironmentVariable(EnvironmentVariables.MySqlConfigUser), System.Environment.GetEnvironmentVariable(EnvironmentVariables.MySqlConfigPw), System.Environment.GetEnvironmentVariable(EnvironmentVariables.MySqlConfigServer)),
                ConfigurationProviderType.Environment => new EnvironmentConfigProvider(),
                _ => throw new ArgumentException($"Unkonwn ConfigProviderType '{BlogConfiguration.ConfigProvider}' was given")
            };
        }

        public static async Task<(IConfigurationRoot Configuration, IConfigProvider ConfigProvider, ConfigPreloader ConfigLoader, string ReddisConnectionString)> InitializeAppAsync()
        {
            IConfigProvider configProvider = GetConfigProvider();
            var configLoader = await ConfigPreloader.LoadAsync(configProvider);
            
            await configLoader.LoadByKeysAsync(ConfigNames.BlogDatabaseName, ConfigNames.JwtSecret, ConfigNames.AdminUsername, ConfigNames.AdminPassword, ConfigNames.RecaptchaSecret);
            if(BlogConfiguration.PersistenceLayer == PersistenceLayerType.MongoDb)
            {
                await configLoader.LoadByKeysAsync(ConfigNames.MongoDbConnectionString);
            }
            else if(BlogConfiguration.PersistenceLayer == PersistenceLayerType.MySql)
            {
                await configLoader.LoadByKeysAsync(ConfigNames.MySqlDbConnectionString);
            }

            if(BlogConfiguration.MessageBus == MessageBusType.RabbitMq)
            {
                await configLoader.LoadByKeysAsync(ConfigNames.RabbitMqHost, ConfigNames.RabbitMqUser, ConfigNames.RabbitMqPassword);
            }
            else if (BlogConfiguration.MessageBus == MessageBusType.AwsSqs || BlogConfiguration.MessageBus == MessageBusType.AwsEventBus)
            {
                await configLoader.LoadByKeysAsync(ConfigNames.AwsMessageBusAccessKeyId, ConfigNames.AwsMessageBusAccessKey);
            }

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{Configuration.Environment.AspNetCoreEnvironment}.json", optional: true)
                .Build();

            var reddisConnectionString = BlogConfiguration.ApplicationCacheType == CacheType.Distributed ? await configProvider.GetAsync(ConfigNames.RedisConnectionString) : string.Empty;
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
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("ntfrex-traceId", typeof(TraceIdLayoutRenderer));
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("ntfrex-user", typeof(UserLayoutRenderer));

            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                logger.Debug("init main");
                logger.Debug($"AspNetCoreEnvironment={Configuration.Environment.AspNetCoreEnvironment}");

                var app = await InitializeAppAsync();
                var host = CreateHost(args, app.Configuration, app.ConfigProvider, app.ConfigLoader, app.ReddisConnectionString);
                await host.RunAsync();
            }
            catch (Exception exception)
            {
                logger.Error(exception, "Stopped program because of exception");
                throw;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        public static IHost CreateHost(string[] args, IConfigurationRoot configuration, IConfigProvider configProvider, ConfigPreloader configLoader, string reddisConnectionString)
        {
            ServerCertificateSelector.Instance.SetConfigProvider(configProvider);

            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging => 
                {
                    logging.ClearProviders();
                    logging.AddOpenTelemetry(options =>
                    {
                        if (!string.IsNullOrEmpty(BlogConfiguration.OtlpLogExporterPath))
                        {
                            options.AddOtlpExporter(options => options.Endpoint = new Uri(BlogConfiguration.OtlpLogExporterPath));
                        }
                    });
                })
                .UseNLog()
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
