using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading;
using System.Threading.Tasks;

namespace NtFreX.Blog
{
    public class CertificateExpiringHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (ServerCertificateSelector.Instance.HasNoValidCertificate())
                return Task.FromResult(HealthCheckResult.Unhealthy("The ssl certificate is expired"));

            if (ServerCertificateSelector.Instance.IsCertificateAboutToExpire())
                return Task.FromResult(HealthCheckResult.Degraded("The ssl certificate is about to expire"));

            return Task.FromResult(HealthCheckResult.Healthy("The ssl certificate is valid"));
        }
    }
}
