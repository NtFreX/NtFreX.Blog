using App.Metrics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NtFreX.Blog.Auth;
using NtFreX.Blog.Data;
using NtFreX.Blog.Services;
using NtFreX.Blog.Web;

namespace NtFreX.Blog
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
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

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = Configuration.GetConnectionString("Redis");
            });

            services.AddAuthorization(options => options.AddPolicy(AuthorizationPolicyNames.OnlyFromLocal, configure => configure.AddRequirements(new OnlyFromLocalAuthorizationRequirement())));
            services.AddSingleton<IAuthorizationHandler, OnlyFromLocalAuthorizationHandler>();
            services.AddTransient<AuthorizationManager>();

            services.AddHttpsRedirection(options =>
            {
                options.HttpsPort = int.Parse(Configuration["Listeners:Ports:HTTPS"]);
            });

            services.AddControllersWithViews();
            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeFolder("/Private", AuthorizationPolicyNames.OnlyFromLocal);
            });

            services.AddTransient<ArticleService>();
            services.AddTransient<CommentService>();
            services.AddTransient<TagService>();


            services.AddTransient<Database>();
            services.AddTransient<ArticleRepository>();
            services.AddTransient<TagRepository>();
            services.AddTransient<VisitorRepository>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();
            app.UseMiddleware<RequestLoggerMiddleware>();

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
            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseResponseCaching();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
