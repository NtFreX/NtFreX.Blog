using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using NtFreX.Blog.Configuration;
using NtFreX.Blog.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NtFreX.Blog.Health
{
    public class DoesReturnArticlesHealthCheck : ApplicationHealthCheck
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly HttpClient httpClient = Configuration.Environment.IsDevelopment() ? new HttpClient(GetIgnoreCertificateValidationHandler()) : new HttpClient();

        public DoesReturnArticlesHealthCheck(ApplicationContextActivityDecorator traceActivityDecorator, IHttpContextAccessor httpContextAccessor)
            : base(traceActivityDecorator)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public override async Task<HealthCheckResult> DoCheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var url = httpContextAccessor.HttpContext.Request.Scheme + "://" + httpContextAccessor.HttpContext.Request.Host;
            var content = await httpClient.GetStringAsync($"{url}/api/article", cancellationToken);
            var articles = JsonConvert.DeserializeObject<IReadOnlyList<ArticleDto>>(content);

            if (articles.Count == 0)
                return HealthCheckResult.Degraded("No article was returned from the web api");

            if (articles.Any(x => x.IsPublished() && string.IsNullOrEmpty(x.Content)))
                return HealthCheckResult.Degraded("At least one published article has no content");

            if (articles.Any(x => x.IsPublished() && string.IsNullOrEmpty(x.Title)))
                return HealthCheckResult.Degraded("At least one published article has no title");

            return HealthCheckResult.Healthy("The web api returns articles");
        }

        private static HttpClientHandler GetIgnoreCertificateValidationHandler()
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true;
            return handler;
        }
    }
}
