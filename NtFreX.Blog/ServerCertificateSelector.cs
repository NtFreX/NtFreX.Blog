using NtFreX.Blog.Configuration;
using NtFreX.ConfigFlow.DotNet;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace NtFreX.Blog
{
    public sealed class ServerCertificateSelector
    {
        public X509Certificate2 currentCertificate;
        private IConfigProvider configProvider;

        public static ServerCertificateSelector Instance { get; } = new ServerCertificateSelector();

        private ServerCertificateSelector()
        { }

        public void SetConfigProvider(IConfigProvider configProvider) 
        { 
            this.configProvider = configProvider; 
        }

        public async ValueTask<X509Certificate2> GetCurrentCertificateAsync()
        {
            if(currentCertificate != null && DateTime.UtcNow - TimeSpan.FromDays(7) > currentCertificate.NotAfter)
            {
                currentCertificate = null;
            }

            if(currentCertificate == null)
            {
                var sslCert = await configProvider.GetAsync(ConfigNames.SslCert);
                var sslCertPw = await configProvider.GetAsync(ConfigNames.SslCertPw);
                currentCertificate = new X509Certificate2(Convert.FromBase64String(sslCert), sslCertPw);
            }

            if(currentCertificate != null && DateTime.UtcNow > currentCertificate.NotAfter)
            {
                currentCertificate = null;
            }

            return currentCertificate;
        }
    }
}
