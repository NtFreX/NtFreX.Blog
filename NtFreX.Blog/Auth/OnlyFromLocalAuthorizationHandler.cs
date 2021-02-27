using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace NtFreX.Blog.Auth
{
    public class OnlyFromLocalAuthorizationHandler : AuthorizationHandler<OnlyFromLocalAuthorizationRequirement>
    {
        private readonly AuthorizationManager authorizationManager;

        public OnlyFromLocalAuthorizationHandler(AuthorizationManager authorizationManager)
        {
            this.authorizationManager = authorizationManager;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OnlyFromLocalAuthorizationRequirement requirement)
        {
            if(!authorizationManager.IsAdmin())
            {
                context.Fail();
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
