using Microsoft.AspNetCore.Mvc;
using NtFreX.Blog.Models;
using NtFreX.Blog.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NtFreX.Blog.Web
{
    [ApiController, Route("api/{controller}")]
    public class CommentController : ControllerBase
    {
        private readonly CommentService commentService;
        private readonly RecaptchaManager recaptchaManager;

        public CommentController(CommentService commentService, RecaptchaManager recaptchaManager)
        {
            this.commentService = commentService;
            this.recaptchaManager = recaptchaManager;
        }

        [HttpGet("byArticleId/{articleId}")]
        public async Task<IReadOnlyList<CommentDto>> GetCommentsByArticleIdAsync(string articleId)
            => await commentService.GetCommentsByArticleIdAsync(articleId);

        [HttpPost()]
        public async Task<IActionResult> InsertCommentAsync([FromBody] CreateCommentDto model)
        {
            if (!await recaptchaManager.ValidateReCaptchaResponseAsync(model.CaptchaResponse))
                return Ok();

            await commentService.InsertCommentAsync(model);
            return Ok();
        }
    }
}
