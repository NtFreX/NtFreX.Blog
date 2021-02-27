using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Net;

namespace NtFreX.Blog.Auth
{
    public class AuthorizationManager
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IHostEnvironment hostEnvironment;

        public AuthorizationManager(IHttpContextAccessor httpContextAccessor, IHostEnvironment hostEnvironment)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.hostEnvironment = hostEnvironment;
        }

        public bool IsAdmin()
        {
            var address = httpContextAccessor.HttpContext.Connection.RemoteIpAddress;
            return IPAddress.IsLoopback(address) && hostEnvironment.IsDevelopment();
        }
    }
}
