using Microsoft.Extensions.Hosting;

namespace NtFreX.Blog.Auth
{
    public static class HostEnvironmentExtensions
    {
        public static bool IsPreProduction(this IHostEnvironment env)
            => env.EnvironmentName == "PreProduction";
    }
}
