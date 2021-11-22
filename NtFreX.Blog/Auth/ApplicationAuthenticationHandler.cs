using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NtFreX.Blog.Configuration;
using NtFreX.Blog.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace NtFreX.Blog.Auth
{
    class ApplicationAuthenticationHandler : AuthenticationHandler<ApplicationAuthenticationOptions>
    {
        private readonly TraceActivityDecorator traceActivityDecorator;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ConfigPreloader configPreloader;
        private readonly ILogger<ApplicationAuthenticationHandler> logger;

        public const string AuthenticationScheme = "Bearer";
        public const string AuthorizationHeader = "Authorization";
        public const string ValidIssuer = "https://ntfrex.com";
        public const string ValidAudience = "https://ntfrex.com";

        public static readonly Counter<int> RequestCount = Program.Meter.CreateCounter<int>("BearerRequestCount", description: "The number of requests handled by the Bearer authentication scheme");
        public static readonly Counter<int> RequestAuthenticatedCount = Program.Meter.CreateCounter<int>("BearerRequestAuthenticatedCount", description: "The number of requests authenticated by the Bearer scheme handler");
        public static readonly Counter<int> RequestUnauthenticatedCount = Program.Meter.CreateCounter<int>("BearerRequestUnauthenticatedCount", description: "The number of requests unauthenticated by the Bearer scheme handler");

        public ApplicationAuthenticationHandler(
            TraceActivityDecorator traceActivityDecorator,
            IOptionsMonitor<ApplicationAuthenticationOptions> options,
            ILoggerFactory loggerFactory,
            UrlEncoder urlEncoder,
            ISystemClock clock,
            IHttpContextAccessor httpContextAccessor, 
            ConfigPreloader configPreloader)
            : base(options, loggerFactory, urlEncoder, clock)
        {
            this.logger = loggerFactory.CreateLogger<ApplicationAuthenticationHandler>();
            this.traceActivityDecorator = traceActivityDecorator;
            this.httpContextAccessor = httpContextAccessor;
            this.configPreloader = configPreloader;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            using var activity = traceActivityDecorator.StartActivity();

            var authenticationResult = TryAuthenticate();

            var tags = new[] {
                new KeyValuePair<string, object>("scheme", AuthenticationScheme),
                new KeyValuePair<string, object>("principal", authenticationResult?.Principal?.GetIdClaim()),
                new KeyValuePair<string, object>("machine", System.Environment.MachineName)
            };

            RequestCount.Add(1, tags);
            RequestAuthenticatedCount.Add(authenticationResult.Succeeded ? 1 : 0, tags);
            RequestUnauthenticatedCount.Add(authenticationResult.Succeeded ? 0 : 1, tags);

            return Task.FromResult(authenticationResult);
        }

        private AuthenticateResult TryAuthenticate()
        {
            var headers = httpContextAccessor?.HttpContext?.Request?.Headers;
            if (headers == null ||
                !headers.ContainsKey(AuthorizationHeader) ||
                headers[AuthorizationHeader].Count == 0)
                return AuthenticateResult.Fail("No authorization header exists in the request header");

            var token = headers[AuthorizationHeader][0].Split(' ');
            if (token.Length != 2)
                return AuthenticateResult.Fail($"The authorization token must start with '{AuthenticationScheme} '");

            var validationResult = IsTokenValid(token[1]);
            if (!validationResult.Success || validationResult.Principal == null)
                return AuthenticateResult.Fail("The authorization token is invalid");

            logger.LogInformation($"The given authorization token is valid, user {validationResult.Principal.GetIdClaim()} was authenticated.");

            var ticket = new AuthenticationTicket(validationResult.Principal, AuthenticationScheme);
            return AuthenticateResult.Success(ticket);
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
