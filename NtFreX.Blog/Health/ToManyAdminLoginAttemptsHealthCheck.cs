using Microsoft.Extensions.Diagnostics.HealthChecks;
using NtFreX.Blog.Cache;
using NtFreX.Blog.Configuration;
using NtFreX.Blog.Logging;
using NtFreX.Blog.Web;
using System.Threading;
using System.Threading.Tasks;

namespace NtFreX.Blog.Health
{
    public class ToManyAdminLoginAttemptsHealthCheck : ApplicationHealthCheck
    {
        private readonly ApplicationCache cache;
        private readonly ConfigPreloader config;

        public ToManyAdminLoginAttemptsHealthCheck(ApplicationCache cache, ConfigPreloader config, TraceActivityDecorator traceActivityDecorator)
            : base(traceActivityDecorator)
        {
            this.cache = cache;
            this.config = config;
        }

        public override async Task<HealthCheckResult> DoCheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var adminUsername = config.Get(ConfigNames.AdminUsername);
            var attempts = await cache.TryGetAsync<int>(CacheKeys.FailedLoginRequests(adminUsername));
            var message = $"There are {attempts.Value} login attempts for the admin user in the last {LoginController.PersistLoginAttemptsForXHours} hours";
            if (attempts.Success && attempts.Value >= LoginController.MaxLoginTries)
                return HealthCheckResult.Degraded(message);

            return HealthCheckResult.Healthy(message);
        }
    }
}
