using System.Linq;
using System.Security.Claims;

namespace NtFreX.Blog.Auth
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetIdClaim(this ClaimsPrincipal principal)
            => principal?.Claims?.FirstOrDefault(x => x.Type == "id")?.Value;
    }
}
