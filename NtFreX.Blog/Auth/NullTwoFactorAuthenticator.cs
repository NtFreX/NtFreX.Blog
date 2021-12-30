using System;
using System.Threading.Tasks;

namespace NtFreX.Blog.Auth
{
    public class NullTwoFactorAuthenticator : ITwoFactorAuthenticator
    {
        public Task SendAndGenerateTwoFactorTokenAsync(string sessionToken, string username)
        {
            throw new NotImplementedException();
        }

        public Task<bool> TryAuthenticateSecondFactor(string sessionToken, string username, string secondFactor)
        {
            throw new NotImplementedException();
        }
    }
}
