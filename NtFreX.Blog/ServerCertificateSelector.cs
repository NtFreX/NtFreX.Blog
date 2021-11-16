using Microsoft.Extensions.Logging;
using NtFreX.Blog.Configuration;
using NtFreX.ConfigFlow.DotNet;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace NtFreX.Blog
{
    public sealed class ServerCertificateSelector
    {
        private X509Certificate2 currentCertificate;
        private IConfigProvider configProvider;
        private ILogger<ServerCertificateSelector> logger;
        private DateTime lastCertificateLoadTime = DateTime.MinValue;

        private const int CertificateIsExpringWhenValidLessThenXDays = 5;
        private const int ReloadCertificateWhenValidLessThenXDays = 7;
        private const int ReloadCertificateAtMostEveryXMinutes = 5;

        private readonly SemaphoreSlim locker = new SemaphoreSlim(1,1);

        public static ServerCertificateSelector Instance { get; } = new ServerCertificateSelector();

        private ServerCertificateSelector()
        { }

        public void SetConfigProvider(IConfigProvider configProvider) => this.configProvider = configProvider;
        public void SetLoggerFactory(ILoggerFactory loggerFactory) => this.logger = loggerFactory.CreateLogger<ServerCertificateSelector>();

        public bool IsCertificateAboutToExpire() => currentCertificate == null ? true : DateTime.UtcNow - TimeSpan.FromDays(CertificateIsExpringWhenValidLessThenXDays) > currentCertificate.NotAfter;
        public bool HasNoValidCertificate() => currentCertificate == null || DateTime.UtcNow > currentCertificate.NotAfter;

        private bool HasNotReloadedCertificateForAShortTime() => lastCertificateLoadTime < DateTime.UtcNow - TimeSpan.FromMinutes(ReloadCertificateAtMostEveryXMinutes);
        private bool IsCertificateExpiringInAShortTime() => DateTime.UtcNow - TimeSpan.FromDays(ReloadCertificateWhenValidLessThenXDays) > currentCertificate.NotAfter;
        private bool IsCertificateExpired() => currentCertificate != null && DateTime.UtcNow > currentCertificate.NotAfter;

        
        public async ValueTask<X509Certificate2> GetCertificateAsync()
        {
            if(currentCertificate != null &&
               IsCertificateExpiringInAShortTime() &&
               HasNotReloadedCertificateForAShortTime())
            {
                // force certificate reload by setting it to null
                logger.LogWarning($"The server ssl certificate is going to expire in less then {ReloadCertificateWhenValidLessThenXDays} days");
                currentCertificate = null;
            }

            if(currentCertificate == null && 
               HasNotReloadedCertificateForAShortTime())
            {
                logger.LogInformation($"Reloading the server ssl certificate, the last time it was loaded was at {lastCertificateLoadTime}");
                
                await locker.WaitAsync();
                await ReloadCertificateIfNotAlreadyDone();
                locker.Release();
            }

            if(IsCertificateExpired())
            {
                // do not use an expire certifcate
                logger.LogWarning($"The server ssl certificate is expired");
                currentCertificate = null;
            }

            return currentCertificate;
        }

        private async Task ReloadCertificateIfNotAlreadyDone()
        {
            if (currentCertificate != null)
                return;

            try
            {
                var sslCert = await configProvider.GetAsync(ConfigNames.SslCert);
                var sslCertPw = await configProvider.GetAsync(ConfigNames.SslCertPw);
                currentCertificate = new X509Certificate2(Convert.FromBase64String(sslCert), sslCertPw);
                lastCertificateLoadTime = DateTime.UtcNow;

                logger.LogInformation($"The new server ssl certificate is going to expire at {currentCertificate?.NotAfter}");
            }
            catch (Exception exce)
            {
                logger.LogError(exce, "Loading the new server ssl certificate failedd");
            }
        }
    }
}
