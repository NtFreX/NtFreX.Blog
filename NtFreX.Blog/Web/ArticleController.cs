using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using NtFreX.Blog.Auth;
using NtFreX.Blog.Data;
using NtFreX.Blog.Models;
using NtFreX.Blog.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NtFreX.Blog.Web
{
    [ApiController, Route("api/{controller}")]
    public class ArticleController : ControllerBase
    {
        private readonly ArticleRepository articleRepository;
        private readonly TagService tagService;
        private readonly VisitorRepository visitorRepository;
        private readonly ArticleService articleService;
        private readonly AuthorizationManager authorizationManager;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IHostEnvironment hostEnvironment;

        public ArticleController(
            ArticleRepository articleRepository,
            TagService tagService,
            VisitorRepository visitorRepository, 
            ArticleService articleService, 
            AuthorizationManager authorizationManager, 
            IHttpContextAccessor httpContextAccessor, 
            IHostEnvironment hostEnvironment)
        {
            this.articleRepository = articleRepository;
            this.tagService = tagService;
            this.visitorRepository = visitorRepository;
            this.articleService = articleService;
            this.authorizationManager = authorizationManager;
            this.httpContextAccessor = httpContextAccessor;
            this.hostEnvironment = hostEnvironment;
        }

        [HttpGet("visit/{articleId}")]
        public async Task VisitAsync(string articleId)
        {
            if (hostEnvironment.IsDevelopment())
                return;

            await visitorRepository.VisitArticleAsync(articleId, httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(), httpContextAccessor.HttpContext.Request.Headers["User-Agent"]);
        }

        // TODO: exclude own visits
        [HttpGet("visitorCount/{articleId}")]
        public async Task<long> GetVisitorCountAsync(string articleId)
            => await visitorRepository.CountVisitorsAsync(articleId);

        [HttpGet("topTree/{excludeId}")]
        public async Task<IReadOnlyList<ArticleDto>> GetTopTreeAsync(string excludeId)
            => await articleService.GetTopTreeArticlesAsync(excludeId);

        [HttpGet("topTreeWithVisitorCount/{excludeId}")]
        public async Task<IReadOnlyList<ArticleWithVisitsDto>> GetTopTreeWithVisitorCountAsync(string excludeId)
            => await articleService.GetTopTreeWithVisitorCountAsync(excludeId);

        [HttpGet("byTag/{tag}")]
        public async Task<IReadOnlyList<ArticleDto>> GetArticlesByTagAsync(string tag)
            => await articleService.GetArticlesByTagAsync(WebHelper.Base64UrlDecode(tag), authorizationManager.IsAdmin());

        [HttpGet("byTagWithVisitorCount/{tag}")]
        public async Task<IReadOnlyList<ArticleWithVisitsDto>> GetAllArticlesWithVisitorCountByTagAsync(string tag)
            => await articleService.GetAllArticlesByTagWithVisitorCountAsync(WebHelper.Base64UrlDecode(tag), authorizationManager.IsAdmin());
        
        [HttpGet]
        public async Task<IReadOnlyList<ArticleDto>> GetAllArticlesAsync()
            => await articleRepository.GetAllArticlesAsync(authorizationManager.IsAdmin());

        [HttpGet("withVisitorCount")]
        public async Task<IReadOnlyList<ArticleWithVisitsDto>> GetAllArticlesWithVisitorCountAsync()
            => await articleService.GetAllArticlesWithVisitorCountAsync(authorizationManager.IsAdmin());

        [HttpGet("{articleId}")]
        public async Task<ArticleDto> GetArticleByIdAsync(string articleId)
        {
            var item = await articleRepository.GetArticleByIdAsync(articleId);
            if (!item.IsPublished() && !authorizationManager.IsAdmin())
                throw new UnauthorizedAccessException();

            return item;
        }

        [HttpPost]
        public async Task<string> CreateArticleAsync()
        {
            if (!authorizationManager.IsAdmin())
                throw new UnauthorizedAccessException();

            return await articleRepository.CreateArticleAsync();
        }

        [HttpPut]
        public async Task SaveArticleAsync(SaveArticleDto model)
        {
            if (!authorizationManager.IsAdmin())
                throw new UnauthorizedAccessException();

            // TODO: because article service needs to load the old tags this line needs to be first. fix this 
            await articleService.SaveArticleAsync(model);
            await tagService.UpdateTagsForArticelAsync(model);
        }
    }
}
