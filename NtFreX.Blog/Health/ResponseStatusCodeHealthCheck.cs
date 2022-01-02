using Microsoft.Extensions.Diagnostics.HealthChecks;
using NtFreX.Blog.Configuration;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NtFreX.Blog.Health
{
    public class ResponseStatusCodeHealthCheck : ApplicationHealthCheck
    {
        public const int DataPoints = 3;
        public const int Max5xxResponseStatusCodes = 0;
        public const int Max4xxResponseStatusCodesInPercent = 10;
        public const float DataPointLengthInMinutes = DataPoints * ResponseStatusCodeHealthCheckMiddleware.MetricPerXSeconds / 60f;

        public ResponseStatusCodeHealthCheck(ApplicationContextActivityDecorator traceActivityDecorator)
            : base(traceActivityDecorator) { }

        public override Task<HealthCheckResult> DoCheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var metrics = ResponseStatusCodeHealthCheckMiddleware.StatusCodeMetrics.GetIncomplete(DataPoints);
            var statusCodes = CombineMetrics(metrics, out var totalRequests);

            foreach (var entry in statusCodes)
            {
                var statusCode = entry.Key.ToString();

                if (HasToMany5xxStatusCodes(statusCode, entry.Value))
                    return Task.FromResult(HealthCheckResult.Degraded($"The server returned {entry.Value} 5xx status codes of {totalRequests} total requests in the last {DataPointLengthInMinutes} minutes"));

                if (HasToMany4xxStatusCodes(statusCode, entry.Value, totalRequests))
                    return Task.FromResult(HealthCheckResult.Degraded($"The server returned {entry.Value} 4xx status codes of {totalRequests} total requests in the last {DataPointLengthInMinutes} minutes"));
            }

            return Task.FromResult(HealthCheckResult.Healthy($"There was no server side failing request in the last {DataPointLengthInMinutes} minutes"));
        }

        private bool HasToMany5xxStatusCodes(string statusCode, ulong count) => statusCode.StartsWith("5") && statusCode.Length == 3 && count > Max5xxResponseStatusCodes;
        private bool HasToMany4xxStatusCodes(string statusCode, ulong count, ulong totalRequests) => statusCode.StartsWith("4") && statusCode.Length == 3 && count / totalRequests * 100 > Max4xxResponseStatusCodesInPercent;

        private Dictionary<int, ulong> CombineMetrics(IEnumerable<ConcurrentDictionary<int, ulong>> metrics, out ulong requestCount)
        {
            var statusCodes = new Dictionary<int, ulong>();
            var requests = 0UL;
            foreach (var metric in metrics)
            {
                foreach (var entry in metric)
                {
                    if (statusCodes.TryGetValue(entry.Key, out var value))
                    {
                        statusCodes[entry.Key] = value + entry.Value;
                    }
                    else
                    {
                        statusCodes.Add(entry.Key, entry.Value);
                    }
                    requests += entry.Value;
                }
            }
            requestCount = requests;
            return statusCodes;
        }
    }
}
