using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NtFreX.Blog.Auth;
using NtFreX.Blog.Models;
using NtFreX.Blog.Services;

namespace NtFreX.Blog.Web
{
    [ApiController, Route("api/[controller]")]
    public class ExportController : ControllerBase
    {
        private readonly AuthorizationManager authorizationManager;
        private readonly ImageService imageService;
        private readonly ArticleService articleService;
        private readonly TagService tagService;
        private readonly CommentService commentService;

        public ExportController(ImageService imageService, ArticleService articleService, TagService tagService, CommentService commentService, AuthorizationManager authorizationManager)
        {
            this.imageService = imageService;
            this.articleService = articleService;
            this.tagService = tagService;
            this.commentService = commentService;
            this.authorizationManager = authorizationManager;
        }

        [HttpGet]
        public async Task<ActionResult> ExportAsync()
        {
            if (!authorizationManager.IsAdmin())
                return Unauthorized();

            var images = await imageService.GetAllAsync();
            var articles = await articleService.GetAllArticlesAsync(includeUnpublished: true);
            var tags = await tagService.GetAllTagsAsync(includeUnpublished: true);
            var comments = await commentService.GetAllCommentsAsync();
            var visitors = await articleService.GetAllVisitorsAsync();

            return Ok(JsonSerializer.Serialize(new ExportDto
            {
                Images = images.ToArray(),
                Articles = articles.ToArray(),
                Tags = tags.ToArray(),
                Comments = comments.ToArray(),
                Visitors = visitors.ToArray()
            }));
        }
    }
}
