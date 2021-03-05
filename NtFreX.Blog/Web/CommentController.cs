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

        public CommentController(CommentService commentService)
        {
            this.commentService = commentService;
        }

        [HttpGet("byArticleId/{articleId}")]
        public async Task<IReadOnlyList<CommentDto>> GetCommentsByArticleIdAsync(string articleId)
            => await commentService.GetCommentsByArticleIdAsync(articleId);

        [HttpPost()]
        public async Task InsertCommentAsync(CreateCommentDto model)
            => await commentService.InsertCommentAsync(model);
    }
}
