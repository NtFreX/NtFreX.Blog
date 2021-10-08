using App.Metrics;
using Firewall;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NtFreX.Blog.Auth;
using NtFreX.Blog.Cache;
using NtFreX.Blog.Data;
using NtFreX.Blog.Services;
using System;

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

            var metrics = AppMetrics.CreateDefaultBuilder().Build();
            services.AddMetrics(metrics);
            services.AddMetricsTrackingMiddleware();
            services.AddHealthChecks()
                    .AddCheck<CertificateExpiringHealthCheck>("SslCertificateHealthCheck");

            if (Blog.Configuration.BlogConfiguration.ApplicationCacheType == Blog.Configuration.CacheType.Distributed)
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                });
            }
            else if(Blog.Configuration.BlogConfiguration.ApplicationCacheType == Blog.Configuration.CacheType.InMemory)
            {
                services.AddMemoryCache();
            }

            services.AddAuthorization(options => options.AddPolicy(AuthorizationPolicyNames.OnlyFromLocal, configure => configure.AddRequirements(new OnlyFromLocalAuthorizationRequirement())));
            services.AddSingleton<IAuthorizationHandler, OnlyFromLocalAuthorizationHandler>();
            services.AddTransient<AuthorizationManager>();

            services.AddControllersWithViews();
            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeFolder("/Private", AuthorizationPolicyNames.OnlyFromLocal);
            });

            services.AddTransient<ApplicationCache>();
            services.AddTransient<ArticleService>();
            services.AddTransient<CommentService>();
            services.AddTransient<TagService>();

            if (Blog.Configuration.BlogConfiguration.PersistenceLayer == Blog.Configuration.PersistenceLayerType.MySql)
            {
                services.AddTransient<MySqlDatabaseConnectionFactory>();
                services.AddTransient<ICommentRepository, RelationalDbCommentRepository>();
                services.AddTransient<IImageRepository, RelationalDbImageRepository>();
                services.AddTransient<IArticleRepository, RelationalDbArticleRepository>();
                services.AddTransient<ITagRepository, RelationalDbTagRepository>();
                services.AddTransient<VisitorRepository, RelationalDbVisitorRepository>();
            }
            else if (Blog.Configuration.BlogConfiguration.PersistenceLayer == Blog.Configuration.PersistenceLayerType.MongoDb)
            {
                services.AddTransient<MongoDatabase>();
                services.AddTransient<ICommentRepository, MongoDbCommentRepository>();
                services.AddTransient<IImageRepository, MongoDbImageRepository>();
                services.AddTransient<IArticleRepository, MongoDbArticleRepository>();
                services.AddTransient<ITagRepository, MongoDbTagRepository>();
                services.AddTransient<VisitorRepository, MongoDbVisitorRepository>();
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
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

            app.UseMetricsAllMiddleware();
            if (Blog.Configuration.BlogConfiguration.ServerSideHttpsRedirection)
            {
                app.UseHttpsRedirection();
            }
            if (Blog.Configuration.BlogConfiguration.ClientSideHttpsRedirection)
            {
                app.UseMiddleware<ClientSideRedirectionMiddleware>();
            }
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();
            app.UseRouting();
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
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
