using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NtFreX.Blog
{
    public class ResponseStatusCodeHealthCheckMiddleware
    {
        public const int MaxMetricItems = 10;
        public const int MetricPerXSeconds = 5 * 60;
        public static readonly MetricCollection<ConcurrentDictionary<int, ulong>> StatusCodeMetrics = new MetricCollection<ConcurrentDictionary<int, ulong>>(MaxMetricItems, MetricPerXSeconds);

        private readonly RequestDelegate next;
        private readonly ILogger<ResponseHeaderMiddleware> logger;

        public ResponseStatusCodeHealthCheckMiddleware(RequestDelegate next, ILogger<ResponseHeaderMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {

            await next(httpContext);

            var statusCode = httpContext.Response.StatusCode;
            logger.LogInformation($"Respone status code will be {statusCode}");

            StatusCodeMetrics.AddOrUpdate(
                () => new ConcurrentDictionary<int, ulong>(new[] { new KeyValuePair<int, ulong>(statusCode, 1) }), 
                metric => metric.AddOrUpdate(statusCode, 1, (key, value) => value + 1));
        }
    }
}