using AutoMapper;
using Dapper;
using Firewall;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NtFreX.Blog.Auth;
using NtFreX.Blog.Cache;
using NtFreX.Blog.Configuration;
using NtFreX.Blog.Data;
using NtFreX.Blog.Data.EfCore;
using NtFreX.Blog.Data.MongoDb;
using NtFreX.Blog.Health;
using NtFreX.Blog.Messaging;
using NtFreX.Blog.Services;
using OpenTelemetry.Contrib.Extensions.AWSXRay.Resources;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StackExchange.Redis;
using System;
using System.IO;

namespace NtFreX.Blog
{
    public class Startup
    {
        private readonly string redisConnectionString;

        public Startup(IConfiguration configuration, string redisConnectionString)
        {
            Configuration = configuration;
            this.redisConnectionString = redisConnectionString;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCaching();
            services.AddResponseCompression();

            services.AddHttpContextAccessor();

            services.AddOpenTelemetryMetrics(builder =>
            {
                builder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddMeter(BlogConfiguration.MetricsName);

                if(BlogConfiguration.UsePrometheusScrapingEndpoint)
                {
                    builder.AddPrometheusExporter();
                }

                if (!string.IsNullOrEmpty(BlogConfiguration.OtlpMetricsExporterPath))
                {
                    builder.AddOtlpExporter(options => options.Endpoint = new Uri(BlogConfiguration.OtlpMetricsExporterPath));
                }
            });

            services.Configure<AspNetCoreInstrumentationOptions>(x =>
            {
                x.Enrich = (activity, eventName, rawObject) =>
                {
                    if (eventName.Equals("OnStartActivity"))
                    {
                        if (rawObject is HttpRequest httpRequest)
                        {
                            var traceID = Guid.NewGuid().ToString();
                            httpRequest.HttpContext.Items[HttpContextItemNames.TraceId] = traceID;

                            activity.SetTag("http.path", httpRequest.Path);
                            activity.SetTag("traceId", traceID);
                            activity.AddTag("aspNetCoreTraceId", httpRequest.HttpContext.TraceIdentifier);
                            foreach (var tag in MetricTags.GetDefaultTags())
                            {
                                activity.SetTag(tag.Key, tag.Value);
                            }
                        }
                    }
                };
            });

            services.AddOpenTelemetryTracing(builder => {
                var resourceBuilder = ResourceBuilder.CreateDefault();

                if (BlogConfiguration.IsAwsEC2)
                    resourceBuilder.AddDetector(new AWSEC2ResourceDetector());
                if (BlogConfiguration.IsAwsEBS)
                    resourceBuilder.AddDetector(new AWSEBSResourceDetector());
                if(BlogConfiguration.IsAwsLambda)
                    resourceBuilder.AddDetector(new AWSLambdaResourceDetector());

                builder
                    .AddXRayTraceId()
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation()
                    .AddAWSInstrumentation()
                    .AddSource(BlogConfiguration.ActivitySourceName)
                    .SetResourceBuilder(resourceBuilder);

                if (!string.IsNullOrEmpty(BlogConfiguration.OtlpTraceExporterPath))
                {
                    builder.AddOtlpExporter(options => options.Endpoint = new Uri(BlogConfiguration.OtlpTraceExporterPath));
                }

                if(BlogConfiguration.ApplicationCacheType == CacheType.Distributed)
                {
                    builder.AddRedisInstrumentation(ConnectionMultiplexer.Connect(redisConnectionString));
                }
            });

            services.AddHealthChecks()
                    .AddCheck<CertificateExpiringHealthCheck>(nameof(CertificateExpiringHealthCheck))
                    .AddCheck<ToManyAdminLoginAttemptsHealthCheck>(nameof(ToManyAdminLoginAttemptsHealthCheck));

            if (BlogConfiguration.ApplicationCacheType == CacheType.Distributed)
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                });
            }
            else if(BlogConfiguration.ApplicationCacheType == CacheType.InMemory)
            {
                services.AddMemoryCache();
            }

            if (BlogConfiguration.MessageBus == MessageBusType.RabbitMq)
            {
                services.AddTransient<IMessageBus, RabbitMessageBus>();
            }
            else if (BlogConfiguration.MessageBus == MessageBusType.AwsSqs)
            {
                services.AddTransient<IMessageBus, AwsSqsMessageBus>();
            }
            else if (BlogConfiguration.MessageBus == MessageBusType.AwsEventBus)
            {
                services.AddTransient<IMessageBus, AwsEventBridgeMessageBus>();
            }
            else
            {
                services.AddTransient<IMessageBus, NullMessageBus>();
            }

            if (BlogConfiguration.EnableTwoFactorAuth)
            {
                services.AddTransient<ITwoFactorAuthenticator, MessageBusTwoFactorAuthenticator>();
            }
            else
            {
                services.AddTransient<ITwoFactorAuthenticator, NullTwoFactorAuthenticator>();
            }

            services.AddAuthorization(options => options.AddPolicy(AuthorizationPolicyNames.OnlyAsAdmin, configure => configure.AddRequirements(new OnlyAsAdminAuthorizationRequirement())));
            services.AddSingleton<IAuthorizationHandler, OnlyAsAdminAuthorizationHandler>();
            services.AddTransient<AuthorizationManager>();

            services.AddAuthentication(ApplicationAuthenticationHandler.AuthenticationScheme)
                    .AddScheme<ApplicationAuthenticationOptions, ApplicationAuthenticationHandler>(ApplicationAuthenticationHandler.AuthenticationScheme, x => { });

            services.AddControllersWithViews(options =>
            {
                // currently there are two write actions which need to be transactional
                //  - updating db
                //  - publishing to event bus
                // the architecture is so that the publish happens last as it cannot be rolled back
                // in case more write actions are added they either need to support rollbacks/journaling
                // or the api needs to be written in an idempotent fashion
                options.Filters.Add<TransactionActionFilter>();
            });

            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeFolder("/Private", AuthorizationPolicyNames.OnlyAsAdmin);
            });

            services.AddTransient<RecaptchaManager>();

            services.AddSingleton(Program.ActivitySource);
            services.AddTransient<ApplicationContextActivityDecorator>();
            services.AddTransient<ApplicationCache>();
            services.AddTransient<ArticleService>();
            services.AddTransient<CommentService>();
            services.AddTransient<TagService>();
            services.AddTransient<ImageService>();

            services.AddSingleton<IMapper>(x => new Mapper(new MapperConfiguration(configExpression => {
                ApplictionMapperConfig.ConfigureAutomapper(configExpression);
                Data.ApplictionMapperConfig.ConfigureAutomapper(configExpression);
            })));

            if (BlogConfiguration.PersistenceLayer == PersistenceLayerType.MySql)
            {
                services.AddScoped<MySqlConnectionFactory>();
                services.AddScoped<IConnectionFactory, MySqlConnectionFactory>();
                services.AddScoped<ICommentRepository, RelationalDbCommentRepository>();
                services.AddScoped<IImageRepository, RelationalDbImageRepository>();
                services.AddScoped<IArticleRepository, RelationalDbArticleRepository>();
                services.AddScoped<ITagRepository, RelationalDbTagRepository>();
                services.AddScoped<IVisitorRepository, RelationalDbVisitorRepository>();
            }
            else if (BlogConfiguration.PersistenceLayer == PersistenceLayerType.MongoDb)
            {
                services.AddScoped<MongoConnectionFactory>();
                services.AddScoped<IConnectionFactory, MongoConnectionFactory>();
                services.AddScoped<ICommentRepository, MongoDbCommentRepository>();
                services.AddScoped<IImageRepository, MongoDbImageRepository>();
                services.AddScoped<IArticleRepository, MongoDbArticleRepository>();
                services.AddScoped<ITagRepository, MongoDbTagRepository>();
                services.AddScoped<IVisitorRepository, MongoDbVisitorRepository>();
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            app.UseMiddleware<ActivityTracingMiddleware>();

            if (env.IsProduction())
            {
                app.UseFirewall(FirewallRulesEngine
                    .DenyAllAccess()
                    .ExceptFromCloudflare()
                    .ExceptFromLocalhost());
            }
            else if(env.IsDevelopment())
            {
                app.UseFirewall(FirewallRulesEngine
                    .DenyAllAccess()
                    .ExceptFromLocalhost());
            }

            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            if (BlogConfiguration.ServerSideHttpsRedirection)
            {
                app.UseHttpsRedirection();
            }
            if (BlogConfiguration.ClientSideHttpsRedirection)
            {
                app.UseMiddleware<ClientSideRedirectionMiddleware>();
            }
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseResponseCaching();
            app.UseMiddleware<ResponseHeaderMiddleware>();

            if (BlogConfiguration.UsePrometheusScrapingEndpoint)
            {
                app.UseOpenTelemetryPrometheusScrapingEndpoint();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapFallbackToPage("/Error", "/Error");
                endpoints.MapFallbackToPage("/NotFound", "/NotFound");
            });

            ServerCertificateSelector.Instance.SetLoggerFactory(loggerFactory);
            if (env.IsDevelopment() && BlogConfiguration.PersistenceLayer == PersistenceLayerType.MySql)
            {
                using var scope = serviceProvider.CreateScope();
                var connectionFactory = scope.ServiceProvider.GetRequiredService<MySqlConnectionFactory>();
                connectionFactory.EnsureTablesExists();

                try
                {
                    foreach (var row in File.ReadAllLines("dev_data.sql"))
                    {
                        if (!string.IsNullOrEmpty(row))
                        {
                            connectionFactory.Connection.Execute(row);
                        }
                    }
                }
                catch
                {
                    /* seed only once */
                }
            }
        }
    }
}
