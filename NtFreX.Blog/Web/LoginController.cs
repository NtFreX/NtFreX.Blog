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
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NtFreX.Blog.Web
{
    [ApiController, Route("api/{controller}")]
    public class LoginController : ControllerBase
    {
        private readonly TraceActivityDecorator traceActivityDecorator;
        private readonly ConfigPreloader configPreloader;
        private readonly ApplicationCache cache;
        private readonly ILogger<LoginController> logger;
        private readonly IMessageBus messageBus;
        private readonly IHttpContextAccessor httpContextAccessor;

        public const int MaxLoginTries = 5;
        public const int PersistLoginAttemptsForXHours = 24;

        private static readonly Counter<int> LoginAttemptsCounter = Program.Meter.CreateCounter<int>($"LoginAttempts", description: "The number of login attempts");
        private static readonly Counter<int> LoginSuccessCounter = Program.Meter.CreateCounter<int>($"LoginSuccess", description: "The number of successful logins");
        private static readonly Counter<int> LoginFailedCounter = Program.Meter.CreateCounter<int>($"LoginFailed", description: "The number of failed logins");

        public LoginController(TraceActivityDecorator traceActivityDecorator, ConfigPreloader configPreloader, ApplicationCache cache, ILogger<LoginController> logger, IMessageBus messageBus, IHttpContextAccessor httpContextAccessor)
        {
            this.traceActivityDecorator = traceActivityDecorator;
            this.configPreloader = configPreloader;
            this.cache = cache;
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
        public async Task<ActionResult> LoginAsync([FromBody] LoginCredentialsDto credentials)
        {
            if (!BlogConfiguration.EnableLogins)
                return Ok();

            using var activity = traceActivityDecorator.StartActivity();
            activity.AddTag("username", credentials.Username);

            logger.LogTrace($"Trying to login user {credentials.Username}");
            var token = await TryAuthenticateUser(credentials);

            var tags = new[] {
                    new KeyValuePair<string, object>("username", credentials.Username),
                    new KeyValuePair<string, object>("machine", System.Environment.MachineName)
            };
            LoginAttemptsCounter.Add(1, tags);
            LoginSuccessCounter.Add(string.IsNullOrEmpty(token) ? 0 : 1, tags);
            LoginFailedCounter.Add(string.IsNullOrEmpty(token) ? 1 : 0, tags);

            if(!string.IsNullOrEmpty(token))
                await messageBus.SendMessageAsync("ntfrex.blog.logins", "user: " + User + ", remoteId: " + httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString());

            logger.LogInformation($"User {credentials.Username} login was {(token == null ? "not" : "")} succesfull");
            return Ok(token);
        }

        private async Task<string> TryAuthenticateUser(LoginCredentialsDto credentials)
        {
            using var activity = traceActivityDecorator.StartActivity();

            if (!BlogConfiguration.EnableLogins)
                return null;

            var secret = configPreloader.Get(ConfigNames.JwtSecret);
            var usersAndPasswords = GetUsersAndPasswords();

            var failedLoginRequests = await cache.TryGetAsync<int>(CacheKeys.FailedLoginRequests(credentials.Username));
            if (failedLoginRequests.Success && failedLoginRequests.Value >= MaxLoginTries)
            {
                logger.LogInformation($"User {credentials.Username} has {failedLoginRequests.Value} failed login attempts in the last {PersistLoginAttemptsForXHours} hour(s) and cannot login");
                return null;
            }

            if (!usersAndPasswords.ContainsKey(credentials.Username) || usersAndPasswords[credentials.Username] != credentials.Password)
            {
                logger.LogInformation($"The given password for the user {credentials.Username} is wrong");
                await cache.SetAsync(CacheKeys.FailedLoginRequests(credentials.Username), failedLoginRequests.Value + 1, TimeSpan.FromHours(PersistLoginAttemptsForXHours));
                return null;
            }
            else
            {
                await cache.SetAsync(CacheKeys.FailedLoginRequests(credentials.Username), 0, TimeSpan.FromDays(1));
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = ApplicationAuthenticationHandler.ValidIssuer,
                Audience = ApplicationAuthenticationHandler.ValidAudience,
                IssuedAt = DateTime.UtcNow,
                Subject = new ClaimsIdentity(new[] { new Claim("id", credentials.Username) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)), SecurityAlgorithms.HmacSha256Signature)
            };
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(securityToken);
            return token;
        }
    }
}
