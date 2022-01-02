using Microsoft.Extensions.Diagnostics.HealthChecks;
using NtFreX.Blog.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NtFreX.Blog.Health
{
    public class ResponseStatusCodeHealthCheck : ApplicationHealthCheck
    {
        public const int DataPoints = 3;
        public const int Max5xxResponseStatusCodes = 0;
        public const int Max4xxResponseStatusCodes = 10;

        public ResponseStatusCodeHealthCheck(ApplicationContextActivityDecorator traceActivityDecorator)
            : base(traceActivityDecorator) { }

        public override Task<HealthCheckResult> DoCheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var metrics = ResponseStatusCodeHealthCheckMiddleware.Metrics.PeekIncomplete(DataPoints, out _);
            var statusCodes = CombineMetrics(metrics);

            foreach (var entry in statusCodes)
            {
                var statusCode = entry.Key.ToString();

                if (HasToMany5xxStatusCodes(statusCode, entry.Value))
                    return Task.FromResult(HealthCheckResult.Degraded($"The server returned 5xx status codes in the last {ResponseStatusCodeHealthCheckMiddleware.MetricPerXMinutes} minutes"));

                if (HasToMany4xxStatusCodes(statusCode, entry.Value))
                    return Task.FromResult(HealthCheckResult.Degraded($"The server returned 4xx status codes in the last {ResponseStatusCodeHealthCheckMiddleware.MetricPerXMinutes} minutes"));
            }

            return Task.FromResult(HealthCheckResult.Healthy($"There was no server side failing request in the last {ResponseStatusCodeHealthCheckMiddleware.MetricPerXMinutes} minutes"));
        }

        private bool HasToMany5xxStatusCodes(string statusCode, ulong count) => statusCode.StartsWith("5") && statusCode.Length == 3 && count > Max5xxResponseStatusCodes;
        private bool HasToMany4xxStatusCodes(string statusCode, ulong count) => statusCode.StartsWith("4") && statusCode.Length == 3 && count > Max4xxResponseStatusCodes;

        private Dictionary<int, ulong> CombineMetrics(ResponseStatusCodeHealthCheckMiddleware.Metric[] metrics)
        {
            var statusCodes = new Dictionary<int, ulong>();
            foreach (var metric in metrics)
            {
                foreach (var entry in metric.StatusCodeCount)
                {
                    if (statusCodes.TryGetValue(entry.Key, out var value))
                    {
                        statusCodes[entry.Key] = value + entry.Value;
                    }
                    else
                    {
                        statusCodes.Add(entry.Key, entry.Value);
                    }
                }
            }
            return statusCodes;
        }
    }
}
