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
                            activity.SetBaggage("http.path", httpRequest.Path);
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
                        
            services.AddAuthorization(options => options.AddPolicy(AuthorizationPolicyNames.OnlyAsAdmin, configure => configure.AddRequirements(new OnlyAsAdminAuthorizationRequirement())));
            services.AddSingleton<IAuthorizationHandler, OnlyAsAdminAuthorizationHandler>();
            services.AddTransient<AuthorizationManager>();

            services.AddAuthentication(ApplicationAuthenticationHandler.AuthenticationScheme)
                    .AddScheme<ApplicationAuthenticationOptions, ApplicationAuthenticationHandler>(ApplicationAuthenticationHandler.AuthenticationScheme, x => { });

            services.AddControllersWithViews();
            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeFolder("/Private", AuthorizationPolicyNames.OnlyAsAdmin);
            });

            services.AddTransient<ApplicationCache>();
            services.AddTransient<ArticleService>();
            services.AddTransient<CommentService>();
            services.AddTransient<TagService>();

            services.AddSingleton<IMapper>(x => new Mapper(new MapperConfiguration(configExpression => {
                ApplictionMapperConfig.ConfigureAutomapper(configExpression);
                Data.ApplictionMapperConfig.ConfigureAutomapper(configExpression);
            })));

            if (BlogConfiguration.PersistenceLayer == PersistenceLayerType.MySql)
            {
                services.AddTransient<MySqlDatabaseConnectionFactory>();
                services.AddTransient<ICommentRepository, RelationalDbCommentRepository>();
                services.AddTransient<IImageRepository, RelationalDbImageRepository>();
                services.AddTransient<IArticleRepository, RelationalDbArticleRepository>();
                services.AddTransient<ITagRepository, RelationalDbTagRepository>();
                services.AddTransient<IVisitorRepository, RelationalDbVisitorRepository>();
            }
            else if (BlogConfiguration.PersistenceLayer == PersistenceLayerType.MongoDb)
            {
                services.AddTransient<MongoDatabase>();
                services.AddTransient<ICommentRepository, MongoDbCommentRepository>();
                services.AddTransient<IImageRepository, MongoDbImageRepository>();
                services.AddTransient<IArticleRepository, MongoDbArticleRepository>();
                services.AddTransient<ITagRepository, MongoDbTagRepository>();
                services.AddTransient<IVisitorRepository, MongoDbVisitorRepository>();
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            ServerCertificateSelector.Instance.SetLoggerFactory(loggerFactory);

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
            app.Use(async (context, next) =>
            {
                context.Response.GetTypedHeaders().CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue
                {
                    Public = true,
                    MaxAge = env.IsDevelopment() ? TimeSpan.FromSeconds(30) : TimeSpan.FromDays(1)
                };
                context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] = new [] { "Accept-Encoding" };

                await next();
            });

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
            });

            // TODO: replace with data layer and ef
            if (env.IsDevelopment() && BlogConfiguration.PersistenceLayer == PersistenceLayerType.MySql)
            {
                var connectionFactory = serviceProvider.GetRequiredService<MySqlDatabaseConnectionFactory>();
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
