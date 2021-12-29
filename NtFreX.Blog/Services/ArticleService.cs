using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MongoDB.Driver;
using NtFreX.Blog.Cache;
using NtFreX.Blog.Configuration;
using NtFreX.Blog.Data;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Services
{
    public class ArticleService
    {
        private readonly ApplicationContextActivityDecorator traceActivityDecorator;
        private readonly IArticleRepository articleRepository;
        private readonly ITagRepository tagRepository;
        private readonly TagService tagService;
        private readonly IVisitorRepository visitorRepository;
        private readonly IMapper mapper;
        private readonly ApplicationCache cache;

        public ArticleService(ApplicationContextActivityDecorator traceActivityDecorator, IArticleRepository articleRepository, ITagRepository tagRepository, TagService tagService, IVisitorRepository visitorRepository, IMapper mapper, ApplicationCache cache) 
        {
            this.traceActivityDecorator = traceActivityDecorator;
            this.articleRepository = articleRepository;
            this.tagRepository = tagRepository;
            this.tagService = tagService;
            this.visitorRepository = visitorRepository;
            this.mapper = mapper;
            this.cache = cache;
        }

        private async Task<IReadOnlyList<ArticleDto>> FindAsync(bool includeUnpublished)
        {
            var models = await articleRepository.FindAsync();
            var dtos = mapper.Map<List<ArticleDto>>(models);
            return dtos.Where(x => includeUnpublished || x.IsPublished()).OrderByDescending(x => x.Date).ToList();
        }

        public async Task<string> CreateArticleAsync()
        {
            var id = await articleRepository.InsertAsync(new ArticleModel());
            
            await cache.RemoveSaveAsync(CacheKeys.AllArticles.Name);
            await cache.RemoveSaveAsync(CacheKeys.AllPublishedArticles.Name);

            return id;
        }

        public async Task<IReadOnlyList<ArticleDto>> GetAllArticlesAsync(bool includeUnpublished)
        {
            using var activity = traceActivityDecorator.StartActivity();
            var cacheKey = includeUnpublished ? CacheKeys.AllArticles : CacheKeys.AllPublishedArticles;

            return await cache.CacheAsync(
                cacheKey.Name,
                cacheKey.TimeToLive,
                () => FindAsync(includeUnpublished));
        }

        public async Task<ArticleDto> GetArticleByIdAsync(string id)
        {
            using var activity = traceActivityDecorator.StartActivity();
            var cacheKey = CacheKeys.Article;

            return await cache.CacheAsync(
                    cacheKey.Name(id),
                    cacheKey.TimeToLive,
                    async () => mapper.Map<ArticleDto>(await articleRepository.FindByIdAsync(id)));
        }

        public async Task SaveArticleAsync(SaveArticleDto model)
        {
            using var activity = traceActivityDecorator.StartActivity();

            await articleRepository.UpdateAsync(mapper.Map<ArticleModel>(model.Article));
            var oldTags = await tagRepository.FindByArticleIdAsync(model.Article.Id);
            foreach (var tag in model.Tags.Concat(oldTags.Select(x => x.Name)))
            {
                await cache.RemoveSaveAsync(CacheKeys.ArticlesByTag.Name(tag));
                await cache.RemoveSaveAsync(CacheKeys.PublishedArticlesByTag.Name(tag));
            }

            await cache.RemoveSaveAsync(CacheKeys.Article.Name(model.Article.Id));
            await cache.RemoveSaveAsync(CacheKeys.AllArticles.Name);
            await cache.RemoveSaveAsync(CacheKeys.AllPublishedArticles.Name);

            // Careful: referencing TagService here could lead to circular references
            await tagService.UpdateTagsForArticelAsync(model);
        }

        public async Task VisitArticleAsync(string id, string remoteIp, string userAgent)
        {
            using var activity = traceActivityDecorator.StartActivity();

            var model = new VisitorModel
            {
                Date = DateTime.Now,
                RemoteIp = remoteIp,
                Article = id,
                UserAgent = userAgent
            };
            await visitorRepository.InsertAsync(model);
            await cache.RemoveSaveAsync(CacheKeys.VisitorsByArticleId.Name(id));
        }

        public async Task<long> CountVisitorsAsync(string id)
        {
            using var activity = traceActivityDecorator.StartActivity();
            var cacheKey = CacheKeys.VisitorsByArticleId;

            return await cache.CacheAsync(
                  cacheKey.Name(id),
                  cacheKey.TimeToLive,
                  () => visitorRepository.CountByArticleIdAsync(id));
        }

        public async Task<IReadOnlyList<ArticleDto>> GetTopTreeArticlesAsync(string excludeId)
        {
            using var activity = traceActivityDecorator.StartActivity();

            var top5 = await GetTopFiveArticlesAsync();
            return top5.Where(x => x.Id.ToString() != excludeId).Take(3).ToList();
        }
        
        public async Task<IReadOnlyList<ArticleDto>> GetArticlesByTagAsync(string tag, bool includeUnpublished)
        {
            using var activity = traceActivityDecorator.StartActivity();
            var cacheKey = includeUnpublished ? CacheKeys.ArticlesByTag : CacheKeys.PublishedArticlesByTag;

            return await cache.CacheAsync(cacheKey.Name(tag), cacheKey.TimeToLive, async () =>
            {
                var tags = await tagRepository.FindAsync();
                var articles = await GetAllArticlesAsync(includeUnpublished);
                return articles.Where(a => tags.Any(t => t.ArticleId == a.Id && t.Name.ToLower() == tag.ToLower())).ToList();
            });
        }

        public async Task<IReadOnlyList<ArticleWithVisitsDto>> GetTopTreeWithVisitorCountAsync(string excludeId)
        {
            using var activity = traceActivityDecorator.StartActivity();
            return await WithVisitorCountAsync(await GetTopTreeArticlesAsync(excludeId));
        }

        public async Task<IReadOnlyList<ArticleWithVisitsDto>> GetAllArticlesByTagWithVisitorCountAsync(string tag, bool includeUnpublished)
        {
            using var activity = traceActivityDecorator.StartActivity();
            return await WithVisitorCountAsync(await GetArticlesByTagAsync(tag, includeUnpublished));
        }

        public async Task<IReadOnlyList<ArticleWithVisitsDto>> GetAllArticlesWithVisitorCountAsync(bool includeUnpublished)
            => await WithVisitorCountAsync(await GetAllArticlesAsync(includeUnpublished));

        private async Task<IReadOnlyList<ArticleDto>> GetTopFiveArticlesAsync()
        {
            // recalculate top 5 articles every day
            using var activity = traceActivityDecorator.StartActivity();
            return await cache.CacheAsync(CacheKeys.Top5Articles.Name, CacheKeys.Top5Articles.TimeToLive, async () =>
            {
                var articles = await FindAsync(includeUnpublished: false);
                var withCount = await Task.WhenAll(articles.Select(async a => new { Article = a, Count = await CountVisitorsAsync(a.Id.ToString()) }));
                return withCount.OrderByDescending(x => x.Count).Select(x => x.Article).Take(5).ToList();
            });
        }

        private async Task<IReadOnlyList<ArticleWithVisitsDto>> WithVisitorCountAsync(IReadOnlyList<ArticleDto> articles)
        {
            using var activity = traceActivityDecorator.StartActivity();
            return await Task.WhenAll(articles.Select(async x => new ArticleWithVisitsDto { Article = x, VisitorCount = await CountVisitorsAsync(x.Id) }).ToList());
        }
    }
}
