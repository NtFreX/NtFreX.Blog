using Microsoft.Extensions.Diagnostics.HealthChecks;
using NtFreX.Blog.Cache;
using NtFreX.Blog.Configuration;
using NtFreX.Blog.Web;
using NtFreX.ConfigFlow.DotNet;
using System.Threading;
using System.Threading.Tasks;

namespace NtFreX.Blog
{
    public class ToManyAdminLoginAttemptsHealthCheck : IHealthCheck
    {
        private readonly ApplicationCache cache;
        private readonly ConfigPreloader config;

        public ToManyAdminLoginAttemptsHealthCheck(ApplicationCache cache, ConfigPreloader config)
        {
            this.cache = cache;
            this.config = config;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
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
