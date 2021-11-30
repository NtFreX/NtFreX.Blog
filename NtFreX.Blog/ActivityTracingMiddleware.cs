using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NtFreX.Blog.Logging;
using System.Threading.Tasks;

namespace NtFreX.Blog
{
    public class ActivityTracingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly TraceActivityDecorator traceActivityDecorator;
        private readonly ILogger<ActivityTracingMiddleware> logger;

        public ActivityTracingMiddleware(RequestDelegate next, TraceActivityDecorator traceActivityDecorator, ILogger<ActivityTracingMiddleware> logger)
        {
            this.next = next;
            this.traceActivityDecorator = traceActivityDecorator;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            using var activity = traceActivityDecorator.StartActivity(name: "NtFreX.Blog.Request");

            var traceId = httpContext.Items[HttpContextItemNames.TraceId];
            using (logger.BeginScope(traceId))
            {
                logger.LogTrace("Begin request scope");
                await next(httpContext);
                logger.LogTrace("End request scope");
            }
        }
    }
}
