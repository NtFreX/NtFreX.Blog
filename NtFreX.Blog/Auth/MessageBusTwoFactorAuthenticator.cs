﻿using Microsoft.Extensions.Logging;
using NtFreX.Blog.Cache;
using NtFreX.Blog.Core;
using NtFreX.Blog.Messaging;
using System;
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
            var twoFactor = RandomExtensions.GetRandomNumberString(TwoFactorLength);

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

            await cache.RemoveSaveAsync(CacheKeys.TwoFactorSession(sessionToken));
            return true;
        }

        private class TwoFactorSession
        {
            public string Username { get; set; }
            public string TwoFactor { get; set; }
            public int Tries { get; set; }
        }
    }
}
