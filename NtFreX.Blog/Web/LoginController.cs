using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NtFreX.Blog.Auth;
using NtFreX.Blog.Cache;
using NtFreX.Blog.Configuration;
using NtFreX.Blog.Logging;
using NtFreX.Blog.Models;
using NtFreX.ConfigFlow.DotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public const int MaxLoginTries = 5;
        public const int PersistLoginAttemptsForXHours = 24; 

        public LoginController(TraceActivityDecorator traceActivityDecorator, ConfigPreloader configPreloader, ApplicationCache cache, ILogger<LoginController> logger)
        {
            this.traceActivityDecorator = traceActivityDecorator;
            this.configPreloader = configPreloader;
            this.cache = cache;
            this.logger = logger;
        }

        private Dictionary<string, string> GetUsersAndPasswords()
            => new Dictionary<string, string>()
            {
                { configPreloader.Get(ConfigNames.AdminUsername), configPreloader.Get(ConfigNames.AdminPassword) }
            };

        [HttpPost]
        public async Task<ActionResult> LoginAsync([FromBody] LoginCredentialsDto credentials)
        {
            var activitySource = new ActivitySource(BlogConfiguration.ActivitySourceName);
            using (var activity = activitySource.StartActivity($"{nameof(LoginController)}.{nameof(LoginAsync)}", ActivityKind.Server))
            {
                logger.LogTrace($"Trying to login user {credentials.Username}");

                traceActivityDecorator.Decorate(activity);
                activity.AddTag("username", credentials.Username);

                var token = await TryAuthenticateUser(credentials);

                var meter = new Meter(BlogConfiguration.MetricsName);
                meter.CreateObservableGauge(
                    $"Login",
                    () => new Measurement<int>(
                        token == null ? 0 : 1,
                        new KeyValuePair<string, object>("username", credentials.Username),
                        new KeyValuePair<string, object>("machine", System.Environment.MachineName)),
                    "0 = cache has not been hit, 1 = cache has been hit");

                logger.LogInformation($"User {credentials.Username} login was {(token == null ? "not" : "")} succesfull");

                return Ok(token);
            }
        }

        private async Task<string> TryAuthenticateUser(LoginCredentialsDto credentials)
        {
            var activitySource = new ActivitySource(BlogConfiguration.ActivitySourceName);
            using (var activity = activitySource.StartActivity($"{nameof(LoginController)}.{nameof(TryAuthenticateUser)}", ActivityKind.Server))
            {
                traceActivityDecorator.Decorate(activity);

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
}
