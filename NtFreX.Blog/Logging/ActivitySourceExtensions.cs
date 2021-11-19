using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NtFreX.Blog.Logging
{
    public static class ActivitySourceExtensions
    {
        public static Activity StartActivity(this TraceActivityDecorator traceActivityDecorator, [CallerMemberName] string name = "", ActivityKind kind = ActivityKind.Server)
        {
            var activity = Program.ActivitySource.StartActivity(name, kind);
            traceActivityDecorator.Decorate(activity);
            return activity;
        }
    }
}
