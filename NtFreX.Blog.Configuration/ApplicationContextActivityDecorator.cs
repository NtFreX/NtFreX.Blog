using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NtFreX.Blog.Configuration
{
    public class ApplicationContextActivityDecorator
    {
        private readonly ActivitySource activitySource;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<ApplicationContextActivityDecorator> logger;

        public ApplicationContextActivityDecorator(ActivitySource activitySource, IHttpContextAccessor httpContextAccessor, ILogger<ApplicationContextActivityDecorator> logger)
        {
            this.activitySource = activitySource;
            this.httpContextAccessor = httpContextAccessor;
            this.logger = logger;
        }

        public Activity StartActivity([CallerMemberName] string name = "", ActivityKind kind = ActivityKind.Server)
        {
            logger.LogDebug($"Starting activity with kind {kind} and name {name}");

            var activity = activitySource.StartActivity(name, kind);

            foreach (var tag in MetricTags.GetDefaultTags())
            {
                activity.AddTag(tag.Key, tag.Value);
            }
            activity.SetTag("traceId", httpContextAccessor.HttpContext.Items[HttpContextItemNames.TraceId]);
            activity.AddTag("aspNetCoreTraceId", httpContextAccessor.HttpContext?.TraceIdentifier);

            return activity;
        }
    }
}
