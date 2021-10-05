using Microsoft.Extensions.Hosting;

namespace NtFreX.Blog.Auth
{
    public class AuthorizationManager
    {
        private readonly IHostEnvironment hostEnvironment;

        public AuthorizationManager(IHostEnvironment hostEnvironment)
        {
            this.hostEnvironment = hostEnvironment;
        }

        public bool IsAdmin()
        {
            return hostEnvironment.IsDevelopment() || hostEnvironment.IsPreProduction();
        }
    }
}
