using NLog;
using NLog.Web.LayoutRenderers;
using System.Text;

namespace NtFreX.Blog.Logging
{
    public class HttpContextItemLayoutRenderer : AspNetLayoutRendererBase
    {
        private readonly string itemName;

        public HttpContextItemLayoutRenderer(string itemName)
        {
            this.itemName = itemName;
        }
        
        protected override void DoAppend(StringBuilder builder, LogEventInfo logEvent)
        {
            if (!(HttpContextAccessor?.HttpContext?.Items?.ContainsKey(itemName) ?? false))
                return;

            var traceId = HttpContextAccessor.HttpContext.Items[itemName].ToString();
            builder.Append(traceId);
        }
    }
}
