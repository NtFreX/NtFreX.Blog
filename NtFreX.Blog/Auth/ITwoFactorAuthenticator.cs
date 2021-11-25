using System.Threading.Tasks;

namespace NtFreX.Blog.Auth
{
    public interface ITwoFactorAuthenticator
    {
        Task SendAndGenerateTwoFactorTokenAsync(string sessionToken, string username);
        Task<bool> TryAuthenticateSecondFactor(string sessionToken, string username, string secondFactor);
    }
}
