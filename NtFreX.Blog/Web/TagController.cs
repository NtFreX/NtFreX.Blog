using Microsoft.AspNetCore.Mvc;
using NtFreX.Blog.Auth;
using NtFreX.Blog.Models;
using NtFreX.Blog.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NtFreX.Blog.Web
{
    [ApiController, Route("api/{controller}")]
    public class TagController : ControllerBase
    {
        private readonly TagService tagService;
        private readonly AuthorizationManager authorizationManager;

        public TagController(TagService tagService, AuthorizationManager authorizationManager)
        {
            this.tagService = tagService;
            this.authorizationManager = authorizationManager;
        }

        [HttpGet("distinctPublished")]
        public async Task<IReadOnlyList<string>> GetAllDistinctPublishedTagsAsync()
            => await tagService.GetAllDistinctTagsAsync(authorizationManager.IsAdmin());

        [HttpGet]
        public async Task<IReadOnlyList<TagDto>> GetAllTagsAsync()
            => await tagService.GetAllTagsAsync(authorizationManager.IsAdmin());

        [HttpGet("byArticleId/{id}")]
        public async Task<IReadOnlyList<TagDto>> GetTagsByArticleIdAsync(string id)
            => await tagService.GetTagsByArticleIdAsync(id);
    }
}
