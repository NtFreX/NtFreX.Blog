using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace NtFreX.Blog
{
    public class ResponseHeaderMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IWebHostEnvironment environment;
        private readonly ILogger<ResponseHeaderMiddleware> logger;

        public ResponseHeaderMiddleware(RequestDelegate next, IWebHostEnvironment environment, ILogger<ResponseHeaderMiddleware> logger)
        {
            this.next = next;
            this.environment = environment;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            logger.LogInformation("Applying custom response headers");
            httpContext.Response.GetTypedHeaders().CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue
            {
                Public = true,
                MaxAge = environment.IsDevelopment() ? TimeSpan.FromSeconds(30) : TimeSpan.FromDays(1)
            };
            httpContext.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] = new[] { "Accept-Encoding" };

            await next(httpContext);
        }
    }
}
