using Microsoft.Extensions.Diagnostics.HealthChecks;
using NtFreX.Blog.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace NtFreX.Blog.Health
{
    public class ResponseStatusCodeHealthCheck : ApplicationHealthCheck
    {
        public const int Max5xxResponseStatusCodes = 0;
        public const int Max4xxResponseStatusCodes = 10;

        public ResponseStatusCodeHealthCheck(ApplicationContextActivityDecorator traceActivityDecorator)
            : base(traceActivityDecorator) { }

        public override Task<HealthCheckResult> DoCheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (!ResponseStatusCodeHealthCheckMiddleware.Metrics.TryPeek(out var metric))
                return Task.FromResult(HealthCheckResult.Healthy("There was no request since the server started"));

            foreach (var entry in metric.StatusCodeCount)
            {
                var textKey = entry.Key.ToString();

                if (textKey.StartsWith("5") && textKey.Length == 3 && entry.Value > Max5xxResponseStatusCodes)
                    return Task.FromResult(HealthCheckResult.Degraded($"The server returned 5xx status codes in the last {ResponseStatusCodeHealthCheckMiddleware.MetricPerXMinutes} minutes"));

                if (textKey.StartsWith("4") && textKey.Length == 3 && entry.Value > Max4xxResponseStatusCodes)
                    return Task.FromResult(HealthCheckResult.Degraded($"The server returned 4xx status codes in the last {ResponseStatusCodeHealthCheckMiddleware.MetricPerXMinutes} minutes"));
            }

            return Task.FromResult(HealthCheckResult.Healthy($"There was no server side failing request in the last {ResponseStatusCodeHealthCheckMiddleware.MetricPerXMinutes} minutes"));
        }
    }
}
