namespace NtFreX.Blog.Logging
{
    public class TraceIdLayoutRenderer : HttpContextItemLayoutRenderer
    {
        public TraceIdLayoutRenderer()
            : base(HttpContextItemNames.TraceId) { }
    }
}
