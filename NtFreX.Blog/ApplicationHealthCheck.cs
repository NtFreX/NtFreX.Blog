using Microsoft.Extensions.Diagnostics.HealthChecks;
using NtFreX.Blog.Configuration;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;

namespace NtFreX.Blog
{
    public abstract class ApplicationHealthCheck : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var activityName = "ApplicationHealthCheck";
            var healthCheckName = GetType().Name;
            var activitySource = new ActivitySource(BlogConfiguration.ActivitySourceName);
            using (var sampleActivity = activitySource.StartActivity(activityName, ActivityKind.Server))
            {
                sampleActivity.AddBaggage("HealthCheckName", healthCheckName);
                sampleActivity.AddBaggage("Environment.MachineName", System.Environment.MachineName);;

                var result = await DoCheckHealthAsync(context, cancellationToken);
                var meter = new Meter(BlogConfiguration.MetricsName);
                meter.CreateObservableGauge($"{activityName}.{healthCheckName}", () => new[] { new Measurement<int>((int)result.Status) }, "HealthStatus", "unhealthy = 0, degraded = 1, healthy = 2");
                
                return result;
            }
        }

        public abstract Task<HealthCheckResult> DoCheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default);
    }
}
