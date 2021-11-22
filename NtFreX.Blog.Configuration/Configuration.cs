namespace NtFreX.Blog.Configuration
{
    public sealed class BlogConfiguration
    {
        public const string BlogTitle = "NTFREX'S SPACE";
        public const string BlogSubtitle = "A small blog about programming and more";
        public const string BlogDescription = "I'm a software developer with a wide range of interests. This blog is mainly to help me improve my writing and explanation skills.";
        public const string BlogOwner = "NtFreX";
        public const string BlogOwnerEmail = "ntfrex (at) gmail (dot) com";
        public const string BlogOwnerProfileImage = "/api/image/profile.jpg";
        public const string TwitterUrl = "https://twitter.com/NtFreX";
        public const string StackOverflowUrl = "https://stackoverflow.com/users/6583901/ntfrex?tab=profile";
        public const string GithubUrl = "https://github.com/NtFreX";
        public const string GoogleSiteVerification = "MAEgvSwMFjwJFm4j5G82JjicSuHql2MVwYxNziXk31c";
        public const string GoogleTagMeasurementId = "G-M4L2YH1KWB";
        public const string GoogleAdPublisherId = "ca-pub-2457513135434532";
        public const string GoogleAdSlotId = "2975613867";
        public const string CloudflareToken = "0ece157d28d74e38adeeaef2f31b9c18";
        public const int MinPageLoadTimeInMs = 100;
        public const bool ClientSideHttpsRedirection = true;
        public const bool ServerSideHttpsRedirection = false;
        public const bool ServiceWorker = false;
        public const PersistenceLayerType PersistenceLayer = PersistenceLayerType.MySql;
        public const ConfigurationProviderType ConfigProvider = ConfigurationProviderType.MySql;
        public const int HttpsPort = 5001;
        public const int HttpPort = 5000;
        public const string MetricsName = "ApplicationMetrics";
        public const string ActivitySourceName = "ApplicationActivitySource";
        public const bool UsePrometheusScrapingEndpoint = false;
        public const bool EnableLogins = false;
        public const string OtlpLogExporterPath = "http://localhost:4317";
        public const string OtlpMetricsExporterPath = "http://localhost:4317";
        public static string OtlpTraceExporterPath => Environment.IsDevelopment() ? "http://localhost:4317" : "http://localhost:2000";
        public const bool IsAwsLambda = false;
        public const bool IsAwsEC2 = false;
        public static bool IsAwsEBS => Environment.IsProduction();
        public static MessageBusType MessageBus => Environment.IsProduction() ? MessageBusType.AwsEventBus : MessageBusType.RabbitMq;
        public static CacheType ApplicationCacheType => Environment.IsProduction() ? CacheType.InMemory : CacheType.Distributed;
    }

    public sealed class Constants
    {
        public const string ClientId = "NtFreX.Blog-064d997e-54ee-4dc1-854c-3c742d2fe54e";
    }

    public sealed class EnvironmentVariables
    {
        public const string AspNetCoreEnvironment = "ASPNETCORE_ENVIRONMENT";
        public const string WebConfigProviderSecret = "NtFrexConfigSecret";
        public const string MySqlConfigUser = "NtFrexMySqlConfigUser";
        public const string MySqlConfigPw = "NtFrexMySqlConfigPw";
        public const string MySqlConfigServer = "NtFrexMySqlConfigServer";
    }

    public static class ConfigNames
    {
        public static string MongoDbConnectionString => SanitizeConfigName("NtFreX.Blog.MongoDbConnectionString");
        public static string RedisConnectionString => SanitizeConfigName("NtFreX.Blog.RedisConnectionString");
        public static string RabbitMqHost => SanitizeConfigName("NtFreX.Blog.RabbitMqHost");
        public static string RabbitMqUser => SanitizeConfigName("NtFreX.Blog.RabbitMqUser");
        public static string RabbitMqPassword => SanitizeConfigName("NtFreX.Blog.RabbitMqPassword");
        public static string AwsMessageBusAccessKeyId => SanitizeConfigName("NtFreX.Blog.AwsMessageBusAccessKeyId");
        public static string AwsMessageBusAccessKey => SanitizeConfigName("NtFreX.Blog.AwsMessageBusAccessKey");

        public static string MySqlDbConnectionString => SanitizeConfigName($"NtFreX.Blog.{Environment.AspNetCoreEnvironment}.MySqlDbConnectionString");
        public static string BlogDatabaseName => SanitizeConfigName("NtFreX.Blog.BlogDatabase");
        public static string SslCertPw => SanitizeConfigName($"NtFreX.Blog.{Environment.AspNetCoreEnvironment}.SslCertPw");
        public static string SslCert => SanitizeConfigName($"NtFreX.Blog.{Environment.AspNetCoreEnvironment}.SslCert");
        public static string AdminUsername => SanitizeConfigName($"NtFreX.Blog.{Environment.AspNetCoreEnvironment}.Admin.Username");
        public static string AdminPassword => SanitizeConfigName($"NtFreX.Blog.{Environment.AspNetCoreEnvironment}.Admin.Password");
        public static string JwtSecret => SanitizeConfigName($"NtFreX.Blog.{Environment.AspNetCoreEnvironment}.Jwt.Secret");

        public static string SanitizeConfigName(string name)
            => BlogConfiguration.ConfigProvider == ConfigurationProviderType.Environment ? name.Replace(".", string.Empty).ToUpper() :
               BlogConfiguration.ConfigProvider == ConfigurationProviderType.Web || BlogConfiguration.ConfigProvider == ConfigurationProviderType.MySql ? name :
               throw new System.ArgumentException($"Unkonwn ConfigProviderType '{BlogConfiguration.ConfigProvider}' was given");
    }

    public sealed class Environment
    {
        public static string AspNetCoreEnvironment => System.Environment.GetEnvironmentVariable(EnvironmentVariables.AspNetCoreEnvironment);

        public static bool IsDevelopment() => AspNetCoreEnvironment == "Development";
        public static bool IsProduction() => AspNetCoreEnvironment == "Production";
    }

    public enum MessageBusType
    {
        None,
        RabbitMq,
        AwsSqs,
        AwsEventBus
    }

    public enum CacheType
    {
        InMemory,
        Distributed
    }

    public enum PersistenceLayerType
    {
        MySql,
        MongoDb
    }

    public enum ConfigurationProviderType
    {
        Environment,
        Web,
        MySql
    }
}
