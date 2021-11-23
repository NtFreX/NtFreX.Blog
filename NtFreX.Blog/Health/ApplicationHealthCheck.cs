using Microsoft.Extensions.Diagnostics.HealthChecks;
using NtFreX.Blog.Configuration;
using NtFreX.Blog.Logging;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NtFreX.Blog.Health
{
    public abstract class ApplicationHealthCheck : IHealthCheck
    {
        private readonly TraceActivityDecorator traceActivityDecorator;

        private static readonly Counter<int> HealthCheckCounter = Program.Meter.CreateCounter<int>($"HealthChecks", description: "The number of health checks");
        private static readonly Counter<int> DegratedCounter = Program.Meter.CreateCounter<int>($"HealthChecks", description: "The number of degrated health checks");
        private static readonly Counter<int> UnhealthyCounter = Program.Meter.CreateCounter<int>($"HealthChecks", description: "The number of unhelathy health checks");
        private static readonly Counter<int> HealthyCounter = Program.Meter.CreateCounter<int>($"HealthChecks", description: "The number of healthy health checks");

        public ApplicationHealthCheck(TraceActivityDecorator traceActivityDecorator)
        {
            this.traceActivityDecorator = traceActivityDecorator;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var healthCheckName = GetType().Name;
            using var activity = traceActivityDecorator.StartActivity();
            activity.AddTag("healthCheckName", healthCheckName);

            var result = await DoCheckHealthAsync(context, cancellationToken);

            var tags = new[] {
                new KeyValuePair<string, object>("name", healthCheckName)
            }.Concat(MetricTags.GetDefaultTags()).ToArray();

            HealthCheckCounter.Add(1, tags);
            DegratedCounter.Add(result.Status == HealthStatus.Degraded ? 1 : 0, tags);
            UnhealthyCounter.Add(result.Status == HealthStatus.Unhealthy ? 1 : 0, tags);
            HealthyCounter.Add(result.Status == HealthStatus.Healthy ? 1 : 0, tags);
                            
            return result;
        }

        public abstract Task<HealthCheckResult> DoCheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default);
    }
}
