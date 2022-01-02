using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NtFreX.Blog
{
    public class ResponseStatusCodeHealthCheckMiddleware
    {
        public class Metric
        {
            public ConcurrentDictionary<int, ulong> StatusCodeCount { get; set; }
            public int StartMinute { get; set; }
        }

        public const int MaxMetricItems = 10;
        public const int MetricPerXMinutes = 15;
        public static readonly FixedCollection<Metric> Metrics = new FixedCollection<Metric>(MaxMetricItems);

        private readonly object lockObj = new object();
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

            lock (lockObj)
            {
                if (!Metrics.TryPeek(out var metric))
                {
                    metric = AddNewMetric(statusCode);
                }
                else if(metric.StartMinute + MetricPerXMinutes <= DateTime.UtcNow.Minute)
                {
                    metric = AddNewMetric(statusCode);
                }
                else
                {
                    metric.StatusCodeCount.AddOrUpdate(statusCode, 1, (key, value) => value + 1);
                }
            }
        }

        private Metric AddNewMetric(int statusCode)
        {
            var currentMinute = DateTime.UtcNow.Minute;
            var metric = new Metric
            {
                StatusCodeCount = new ConcurrentDictionary<int, ulong>(new [] { new KeyValuePair<int, ulong>(statusCode, 1) }),
                StartMinute = currentMinute - currentMinute % MetricPerXMinutes
            };
            Metrics.Add(metric);
            return metric;
        }
    }
}