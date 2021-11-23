using NLog;
using NLog.Web.LayoutRenderers;
using NtFreX.Blog.Auth;
using System.Text;

namespace NtFreX.Blog.Logging
{
    public class UserLayoutRenderer : AspNetLayoutRendererBase
    {
        protected override void DoAppend(StringBuilder builder, LogEventInfo logEvent)
        {
            var claim = HttpContextAccessor?.HttpContext?.User.GetIdClaim();
            if (claim == null)
            {
                builder.Append("anonymous");
            }
            else
            {
                builder.Append(claim);
            }
        }
    }
}
