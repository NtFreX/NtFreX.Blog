using Microsoft.Extensions.Logging;
using NtFreX.Blog.Cache;
using NtFreX.Blog.Messaging;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace NtFreX.Blog.Auth
{
    public class MessageBusTwoFactorAuthenticator : ITwoFactorAuthenticator
    {
        private const int MaxTwoFactorTries = 3;
        private const int TwoFactorLivetimeInMin = 5;
        private const int TwoFactorLength = 5;
        private const string MessageBusName = "ntfrex.blog.twofactor";

        private readonly ApplicationCache cache;
        private readonly ILogger<MessageBusTwoFactorAuthenticator> logger;
        private readonly IMessageBus messageBus;

        public MessageBusTwoFactorAuthenticator(ApplicationCache cache, ILogger<MessageBusTwoFactorAuthenticator> logger, IMessageBus messageBus)
        {
            this.cache = cache;
            this.logger = logger;
            this.messageBus = messageBus;
        }

        public async Task SendAndGenerateTwoFactorTokenAsync(string sessionToken, string username)
        {
            var twoFactor = new string(Enumerable.Repeat(0, TwoFactorLength).Select(x => RandomNumberChar()).ToArray());

            await cache.SetAsync(CacheKeys.TwoFactorSession(sessionToken), new TwoFactorSession { TwoFactor = twoFactor, Username = username }, TimeSpan.FromMinutes(TwoFactorLivetimeInMin));
            await messageBus.SendMessageAsync(MessageBusName, JsonSerializer.Serialize(new { Value = twoFactor }));
        }

        public async Task<bool> TryAuthenticateSecondFactor(string sessionToken, string username, string secondFactor)
        {
            var session = await cache.TryGetAsync<TwoFactorSession>(CacheKeys.TwoFactorSession(sessionToken));
            if (!session.Success || session.Value == null)
            {
                logger.LogWarning($"No two factor session for the key {sessionToken} exists");
                return false;
            }

            session.Value.Tries++;
            if (session.Value.Tries > MaxTwoFactorTries)
            {
                logger.LogWarning($"The given two factor session {sessionToken} was entered wrong for {session.Value.Tries} times");
                return false;
            }

            if (session.Value.Username != username || session.Value.TwoFactor != secondFactor)
            {
                logger.LogWarning($"The given two factor session {sessionToken} registered for {session.Value.Username} does not match the given user {username} or token");
                await cache.SetAsync(CacheKeys.TwoFactorSession(sessionToken), session, TimeSpan.FromMinutes(TwoFactorLivetimeInMin));
                return false;
            }

            return true;
        }

        private char RandomNumberChar()
        {
            using var rg = new RNGCryptoServiceProvider();
            byte[] rno = new byte[5];
            rg.GetBytes(rno);
            return new Random(BitConverter.ToInt32(rno, 0)).Next(0, 10).ToString()[0];
        }

        private class TwoFactorSession
        {
            public string Username { get; set; }
            public string TwoFactor { get; set; }
            public int Tries { get; set; }
        }
    }
}
