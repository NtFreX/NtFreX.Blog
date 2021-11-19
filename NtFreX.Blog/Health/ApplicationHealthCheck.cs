using Microsoft.Extensions.Diagnostics.HealthChecks;
using NtFreX.Blog.Configuration;
using NtFreX.Blog.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;

namespace NtFreX.Blog.Health
{
    public abstract class ApplicationHealthCheck : IHealthCheck
    {
        private readonly TraceActivityDecorator traceActivityDecorator;

        public ApplicationHealthCheck(TraceActivityDecorator traceActivityDecorator)
        {
            this.traceActivityDecorator = traceActivityDecorator;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var activityName = "ApplicationHealthCheck";
            var healthCheckName = GetType().Name;
            var activitySource = new ActivitySource(BlogConfiguration.ActivitySourceName);
            using (var activity = activitySource.StartActivity(activityName, ActivityKind.Server))
            {
                traceActivityDecorator.Decorate(activity);

                activity.AddTag("healthCheckName", healthCheckName);

                var result = await DoCheckHealthAsync(context, cancellationToken);
                var meter = new Meter(BlogConfiguration.MetricsName);
                meter.CreateObservableGauge(
                    $"HealthCheckStatus", 
                    () => new Measurement<int>(
                        (int)result.Status,
                        new KeyValuePair<string, object>("name", healthCheckName),
                        new KeyValuePair<string, object>("machine", System.Environment.MachineName)),
                    "unhealthy = 0, degraded = 1, healthy = 2");
                
                return result;
            }
        }

        public abstract Task<HealthCheckResult> DoCheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default);
    }
}
