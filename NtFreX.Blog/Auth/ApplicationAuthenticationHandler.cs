using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NtFreX.Blog.Configuration;
using NtFreX.ConfigFlow.DotNet;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace NtFreX.Blog.Auth
{
    class ApplicationAuthenticationHandler : AuthenticationHandler<ApplicationAuthenticationOptions>
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ConfigPreloader configPreloader;
        private readonly ILogger<ApplicationAuthenticationHandler> logger;

        public const string AuthenticationScheme = "Bearer";
        public const string AuthorizationHeader = "Authorization";
        public const string ValidIssuer = "https://ntfrex.com";
        public const string ValidAudience = "https://ntfrex.com";

        public ApplicationAuthenticationHandler(
            IOptionsMonitor<ApplicationAuthenticationOptions> options,
            ILoggerFactory loggerFactory,
            UrlEncoder urlEncoder,
            ISystemClock clock,
            IHttpContextAccessor httpContextAccessor, 
            ConfigPreloader configPreloader)
            : base(options, loggerFactory, urlEncoder, clock)
        {
            this.logger = loggerFactory.CreateLogger<ApplicationAuthenticationHandler>();
            this.httpContextAccessor = httpContextAccessor;
            this.configPreloader = configPreloader;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var headers = httpContextAccessor?.HttpContext?.Request?.Headers;
            if (headers == null || 
                !headers.ContainsKey(AuthorizationHeader) ||
                headers[AuthorizationHeader].Count == 0)
                return Task.FromResult(AuthenticateResult.Fail("No authorization header exists in the request header"));

            var token = headers[AuthorizationHeader][0].Split(' ');
            if (token.Length != 2)
                return Task.FromResult(AuthenticateResult.Fail($"The authorization token must start with '{AuthenticationScheme} '"));

            var validationResult = IsTokenValid(token[1]);
            if (!validationResult.Success || validationResult.Principal == null)
                return Task.FromResult(AuthenticateResult.Fail("The authorization token is invalid"));

            logger.LogInformation($"The given authorization token is valid, user {validationResult.Principal.Claims.First(x => x.Type == "id").Value} was authenticated.");

            var ticket = new AuthenticationTicket(validationResult.Principal, AuthenticationScheme);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        private (bool Success, ClaimsPrincipal Principal) IsTokenValid(string token)
        {
            try
            {
                var secret = configPreloader.Get(ConfigNames.JwtSecret);
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidAudience = ValidAudience,
                    ValidIssuer = ValidIssuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret))
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken securityToken);
                if (principal == null)
                    return (false, null);
                if (securityToken == null)
                    return (false, null);

                return (true, principal);
            }
            catch (Exception exce)
            {
                logger.LogWarning(exce, "Validating the authorization token failed");
                return (false, null);
            }
        }
    }
}
