using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace NtFreX.Blog.Logging
{
    public class TraceActivityDecorator
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public TraceActivityDecorator(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public void Decorate(Activity activity)
        {
            activity.AddTag("machine", System.Environment.MachineName);
            activity.AddTag("aspNetCoreTraceId", httpContextAccessor.HttpContext?.TraceIdentifier);
        }
    }
}
