using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NtFreX.Blog.Auth;
using NtFreX.Blog.Cache;
using NtFreX.Blog.Configuration;
using NtFreX.Blog.Models;
using NtFreX.ConfigFlow.DotNet;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NtFreX.Blog.Web
{
    [ApiController, Route("api/{controller}")]
    public class LoginController : ControllerBase
    {
        private readonly ConfigPreloader configPreloader;
        private readonly ApplicationCache cache;

        private const int MaxLoginTries = 5;

        public LoginController(ConfigPreloader configPreloader, ApplicationCache cache)
        {
            this.configPreloader = configPreloader;
            this.cache = cache;
        }

        private Dictionary<string, string> GetUsersAndPasswords()
            => new Dictionary<string, string>()
            {
                { configPreloader.Get(ConfigNames.AdminUsername), configPreloader.Get(ConfigNames.AdminPassword) }
            };

        [HttpPost]
        public async Task<ActionResult> LoginAsync([FromBody] LoginCredentialsDto credentials)
        {
            var secret = configPreloader.Get(ConfigNames.JwtSecret);
            var usersAndPasswords = GetUsersAndPasswords();

            var failedLoginRequests = await cache.TryGetAsync<int>(CacheKeys.FailedLoginRequests(credentials.Username));
            if (failedLoginRequests.Success && failedLoginRequests.Value >= MaxLoginTries)
                return Ok();

            if (!usersAndPasswords.ContainsKey(credentials.Username) || usersAndPasswords[credentials.Username] != credentials.Password)
            {
                await cache.SetAsync(CacheKeys.FailedLoginRequests(credentials.Username), failedLoginRequests.Value + 1, TimeSpan.FromDays(1));
                return Ok();
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
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(tokenHandler.WriteToken(token));
        }
    }
}
