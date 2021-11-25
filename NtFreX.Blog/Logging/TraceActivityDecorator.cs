using Microsoft.AspNetCore.Http;
using NtFreX.Blog.Configuration;
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
            foreach(var tag in MetricTags.GetDefaultTags())
            {
                activity.AddTag(tag.Key, tag.Value);
            }
            activity.SetTag("traceId", httpContextAccessor.HttpContext.Items[HttpContextItemNames.TraceId]);
            activity.AddTag("aspNetCoreTraceId", httpContextAccessor.HttpContext?.TraceIdentifier);
        }
    }
}
