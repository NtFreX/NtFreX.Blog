using NtFreX.Blog.Configuration;
using NtFreX.ConfigFlow.DotNet;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace NtFreX.Blog
{
    public sealed class ServerCertificateSelector
    {
        private X509Certificate2 currentCertificate;
        private IConfigProvider configProvider;
        private DateTime lastCertificateLoadTime = DateTime.MinValue;

        private const int CertificateIsExpringWhenValidLessThenXDays = 5;
        private const int ReloadCertificateWhenValidLessThenXDays = 7;
        private const int ReloadCertificateAtMostEveryXMinutes = 5;

        public static ServerCertificateSelector Instance { get; } = new ServerCertificateSelector();

        private ServerCertificateSelector()
        { }

        public void SetConfigProvider(IConfigProvider configProvider) 
        { 
            this.configProvider = configProvider; 
        }

        public bool IsCertificateAboutToExpire() => currentCertificate == null ? true : DateTime.UtcNow - TimeSpan.FromDays(CertificateIsExpringWhenValidLessThenXDays) > currentCertificate.NotAfter;
        public bool IsCertificateExpired() => currentCertificate == null || DateTime.UtcNow > currentCertificate.NotAfter;

        public async ValueTask<X509Certificate2> GetCurrentCertificateAsync()
        {
            if(currentCertificate != null && DateTime.UtcNow - TimeSpan.FromDays(ReloadCertificateWhenValidLessThenXDays) > currentCertificate.NotAfter)
            {
                currentCertificate = null;
            }

            if(currentCertificate == null && lastCertificateLoadTime < DateTime.UtcNow - TimeSpan.FromMinutes(ReloadCertificateAtMostEveryXMinutes))
            {
                var sslCert = await configProvider.GetAsync(ConfigNames.SslCert);
                var sslCertPw = await configProvider.GetAsync(ConfigNames.SslCertPw);
                currentCertificate = new X509Certificate2(Convert.FromBase64String(sslCert), sslCertPw);
                lastCertificateLoadTime = DateTime.UtcNow;
            }

            if(currentCertificate != null && DateTime.UtcNow > currentCertificate.NotAfter)
            {
                currentCertificate = null;
            }

            return currentCertificate;
        }
    }
}
