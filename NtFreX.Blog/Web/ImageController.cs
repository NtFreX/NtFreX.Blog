using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NtFreX.Blog.Auth;
using NtFreX.Blog.Services;

namespace NtFreX.Blog.Web
{
    [ApiController, Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly ImageService imageService;
        private readonly AuthorizationManager authorizationManager;

        public ImageController(ImageService imageService, AuthorizationManager authorizationManager)
        {
            this.imageService = imageService;
            this.authorizationManager = authorizationManager;
        }

        [HttpGet, Route("names")]
        public async Task<ActionResult> GetNamesAsync()
        {
            if (!authorizationManager.IsAdmin())
                return Unauthorized();

            return Ok(await imageService.GetAllNamesAsync());
        }

        [HttpGet, Route("{name}")]
        [ResponseCache(Duration = 60 * 60 * 24)]
        public async Task<ActionResult> GetAsync(string name)
        {
            var image = await imageService.GetByNameAsync(name);
            var stream = imageService.ToStream(image);
            return new FileStreamResult(stream, image.Type);
        }

        [HttpPost, Route("{name}")]
        public async Task<ActionResult> PostAsync(string name)
        {
            if (!authorizationManager.IsAdmin())
                return Unauthorized();

            await imageService.AddAsync(name, Request.Body);

            return Ok();
        }
    }
}
