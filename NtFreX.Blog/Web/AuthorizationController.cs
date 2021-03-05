using Microsoft.AspNetCore.Mvc;
using NtFreX.Blog.Auth;

namespace NtFreX.Blog.Web
{
    [ApiController, Route("api/{controller}")]
    public class AuthorizationController : ControllerBase
    {
        private readonly AuthorizationManager authorizationManager;

        public AuthorizationController(AuthorizationManager authorizationManager)
        {
            this.authorizationManager = authorizationManager;
        }

        [HttpGet("isAdmin")]
        public bool IsAdmin()
            => authorizationManager.IsAdmin();
    }
}
