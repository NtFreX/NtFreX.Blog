using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NtFreX.Blog.Auth;
using NtFreX.Blog.Cache;
using NtFreX.Blog.Configuration;
using NtFreX.Blog.Logging;
using NtFreX.Blog.Messaging;
using NtFreX.Blog.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NtFreX.Blog.Web
{
    [ApiController, Route("api/{controller}")]
    public class LoginController : ControllerBase
    {
        private readonly TraceActivityDecorator traceActivityDecorator;
        private readonly ConfigPreloader configPreloader;
        private readonly ApplicationCache cache;
        private readonly RecaptchaManager recaptchaManager;
        private readonly ITwoFactorAuthenticator twoFactorAuthenticator;
        private readonly ILogger<LoginController> logger;
        private readonly IMessageBus messageBus;
        private readonly IHttpContextAccessor httpContextAccessor;

        public const int MaxLoginTries = 5;
        public const int PersistLoginAttemptsForXHours = 24;

        private static readonly Counter<int> LoginAttemptsCounter = Program.Meter.CreateCounter<int>($"LoginAttempts", description: "The number of login attempts");
        private static readonly Counter<int> LoginSuccessCounter = Program.Meter.CreateCounter<int>($"LoginSuccess", description: "The number of successful logins");
        private static readonly Counter<int> LoginFailedCounter = Program.Meter.CreateCounter<int>($"LoginFailed", description: "The number of failed logins");

        public LoginController(
            TraceActivityDecorator traceActivityDecorator, 
            ConfigPreloader configPreloader, 
            ApplicationCache cache,
            RecaptchaManager recaptchaManager,
            ITwoFactorAuthenticator twoFactorAuthenticator,
            ILogger<LoginController> logger, 
            IMessageBus messageBus, 
            IHttpContextAccessor httpContextAccessor)
        {
            this.traceActivityDecorator = traceActivityDecorator;
            this.configPreloader = configPreloader;
            this.cache = cache;
            this.recaptchaManager = recaptchaManager;
            this.twoFactorAuthenticator = twoFactorAuthenticator;
            this.logger = logger;
            this.messageBus = messageBus;
            this.httpContextAccessor = httpContextAccessor;
        }

        private Dictionary<string, string> GetUsersAndPasswords()
            => new Dictionary<string, string>()
            {
                { configPreloader.Get(ConfigNames.AdminUsername), configPreloader.Get(ConfigNames.AdminPassword) }
            };

        [HttpPost]
        public async Task<LoginResponseDto> LoginAsync([FromBody] LoginCredentialsDto credentials)
        {
            using var activity = traceActivityDecorator.StartActivity();
            activity.AddTag("username", credentials.Key);
            activity.AddTag("session", credentials.Session);
            activity.AddTag("type", credentials.Type);

            logger.LogTrace($"Trying to login user {credentials.Key}");

            var response = await RunAllAuthenticationChecksAsync(credentials);

            var tags = new[] {
                    new KeyValuePair<string, object>("username", credentials.Key),
                    new KeyValuePair<string, object>("session", credentials.Session),
                    new KeyValuePair<string, object>("type", credentials.Type)
            }.Concat(MetricTags.GetDefaultTags()).ToArray();

            LoginAttemptsCounter.Add(1, tags);
            LoginSuccessCounter.Add(response.Success ? 1 : 0, tags);
            LoginFailedCounter.Add(response.Success ? 0 : 1, tags);

            if (response.Success && response.Type == LoginResponseType.AuthenticationToken) 
            {
                var message = new {
                    User = credentials.Key, 
                    RemoteId = httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString()
                };
                await messageBus.SendMessageAsync("ntfrex.blog.logins", JsonSerializer.Serialize(message));
            }

            logger.LogInformation($"User {credentials.Key} login was {(!response.Success ? "not" : "")} succesfull");
            return response;
        }

        private async Task<LoginResponseDto> RunAllAuthenticationChecksAsync(LoginCredentialsDto credentials)
        {
            if (!await recaptchaManager.ValidateReCaptchaResponseAsync(credentials.CaptchaResponse))
                return LoginResponseDto.Failed();

            if (!BlogConfiguration.EnableLogins)
            {
                logger.LogDebug($"Logins are disabled");
                return LoginResponseDto.Failed();
            }

            if (credentials.Type == LoginCredentialsType.UsernamePassword)
            {
                logger.LogDebug($"authenticating by username and password");

                var canAuthenticate = await TryAuthenticateUser(credentials);
                if(!canAuthenticate)
                    return LoginResponseDto.Failed();

                if (BlogConfiguration.EnableTwoFactorAuth)
                {
                    logger.LogDebug($"generating and sending two factor token");

                    var session = Guid.NewGuid().ToString();
                    await twoFactorAuthenticator.SendAndGenerateTwoFactorTokenAsync(session, credentials.Key);
                    return new LoginResponseDto
                    {
                        Type = LoginResponseType.TwoFactorToken,
                        Success = true,
                        Value = session
                    };
                }
                else
                {
                    logger.LogDebug($"generating and returning auth token");

                    var token = GenerateAuthenticationToken(credentials.Key);
                    return new LoginResponseDto
                    {
                        Type = LoginResponseType.AuthenticationToken,
                        Success = true,
                        Value = token
                    };
                }
            }
            else if (credentials.Type == LoginCredentialsType.TwoFactor)
            {
                logger.LogDebug($"authenticating by two factor token");

                if (await twoFactorAuthenticator.TryAuthenticateSecondFactor(credentials.Session, credentials.Key, credentials.Secret))
                {
                    logger.LogDebug($"generating and returning auth token");

                    var token = GenerateAuthenticationToken(credentials.Key);
                    return new LoginResponseDto
                    {
                        Type = LoginResponseType.AuthenticationToken,
                        Success = true,
                        Value = token
                    };
                }
            }
            return LoginResponseDto.Failed();
        }

        private async Task<bool> TryAuthenticateUser(LoginCredentialsDto credentials)
        {
            using var activity = traceActivityDecorator.StartActivity();

            var usersAndPasswords = GetUsersAndPasswords();
            var failedLoginRequests = await cache.TryGetAsync<int>(CacheKeys.FailedLoginRequests(credentials.Key));
            if (failedLoginRequests.Success && failedLoginRequests.Value >= MaxLoginTries)
            {
                logger.LogInformation($"User {credentials.Key} has {failedLoginRequests.Value} failed login attempts in the last {PersistLoginAttemptsForXHours} hour(s) and cannot login");
                return false;
            }

            if (!usersAndPasswords.ContainsKey(credentials.Key) || usersAndPasswords[credentials.Key] != credentials.Secret)
            {
                logger.LogInformation($"The given password for the user {credentials.Key} is wrong");
                await cache.SetAsync(CacheKeys.FailedLoginRequests(credentials.Key), failedLoginRequests.Value + 1, TimeSpan.FromHours(PersistLoginAttemptsForXHours));
                return false;
            }

            await cache.SetAsync(CacheKeys.FailedLoginRequests(credentials.Key), 0, TimeSpan.FromDays(1));
            return true;            
        }

        private string GenerateAuthenticationToken(string username)
        {
            logger.LogTrace("Generating an access token");

            var secret = configPreloader.Get(ConfigNames.JwtSecret);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = ApplicationAuthenticationHandler.ValidIssuer,
                Audience = ApplicationAuthenticationHandler.ValidAudience,
                IssuedAt = DateTime.UtcNow,
                Subject = new ClaimsIdentity(new[] { new Claim("id", username) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)), SecurityAlgorithms.HmacSha256Signature)
            };
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(securityToken);

            logger.LogInformation("The login was successful and a token has been generated");
            return token;
        }

    }
}
