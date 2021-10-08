using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace NtFreX.Blog
{
    public class ClientSideRedirectionMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;

        public ClientSideRedirectionMiddleware(RequestDelegate next, ILogger<ClientSideRedirectionMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Scheme.ToLower() == "http")
            {
                logger.LogWarning("Responding with client side redirection html and stopping request");
                await httpContext.Response.WriteAsync(
@"<html><head><script type='text/javascript'>
    window.location = 'https://' + window.location.host + window.location.pathname + window.location.search + window.location.hash;
</script></head></html>");
            }
            else
            {
                await next(httpContext);
            }
        }
    } 
}
