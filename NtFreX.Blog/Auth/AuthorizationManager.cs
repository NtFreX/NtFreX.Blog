using Microsoft.AspNetCore.Http;
using NtFreX.Blog.Configuration;
using NtFreX.ConfigFlow.DotNet;
using System.Linq;

namespace NtFreX.Blog.Auth
{
    public class AuthorizationManager
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ConfigPreloader configPreloader;

        public AuthorizationManager(IHttpContextAccessor httpContextAccessor, ConfigPreloader configPreloader)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.configPreloader = configPreloader;
        }

        public bool IsAdmin()
        {
            if (httpContextAccessor?.HttpContext?.User?.Claims == null)
                return false;

            var adminUser = configPreloader.Get(ConfigNames.AdminUsername);
            if (httpContextAccessor.HttpContext.User.Claims.Any(x => x.Type == "id" && x.Value == adminUser))
                return true;

            return false;
        }
    }
}
