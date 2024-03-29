﻿using Microsoft.AspNetCore.Http;
using NtFreX.Blog.Configuration;

namespace NtFreX.Blog.Auth
{
    public class AuthorizationManager
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ConfigPreloader configPreloader;

        public AuthorizationManager(IHttpContextAccessor httpContextAccessor, ConfigPreloader configPreloader)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.configPreloader = configPreloader;
        }

        public bool IsAdmin()
        {
            if (httpContextAccessor?.HttpContext?.User?.Claims == null)
                return false;

            var adminUser = configPreloader.Get(ConfigNames.AdminUsername);
            if (httpContextAccessor.HttpContext.User.GetIdClaim() == adminUser)
                return true;

            return false;
        }
    }
}
