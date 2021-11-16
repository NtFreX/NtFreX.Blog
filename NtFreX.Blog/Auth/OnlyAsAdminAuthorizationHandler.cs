using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace NtFreX.Blog.Auth
{
    public class OnlyAsAdminAuthorizationHandler : AuthorizationHandler<OnlyAsAdminAuthorizationRequirement>
    {
        private readonly AuthorizationManager authorizationManager;

        public OnlyAsAdminAuthorizationHandler(AuthorizationManager authorizationManager)
        {
            this.authorizationManager = authorizationManager;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OnlyAsAdminAuthorizationRequirement requirement)
        {
            if (!authorizationManager.IsAdmin())
            {
                context.Fail();
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
