using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;
using NtFreX.Blog.Cache;
using NtFreX.Blog.Data;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Services
{
    public class ArticleService
    {
        private readonly ArticleRepository articleRepository;
        private readonly TagRepository tagRepository;
        private readonly TagService tagService;
        private readonly VisitorRepository visitorRepository;
        private readonly IDistributedCache cache;

        public ArticleService(ArticleRepository articleRepository, TagRepository tagRepository, TagService tagService, VisitorRepository visitorRepository, IDistributedCache cache) 
        {
            this.articleRepository = articleRepository;
            this.tagRepository = tagRepository;
            this.tagService = tagService;
            this.visitorRepository = visitorRepository;
            this.cache = cache;
        }

        public async Task<string> CreateArticleAsync()
        {
            var id = await articleRepository.InsertArticleAsync();
            
            await cache.RemoveAsync(CacheKeys.AllArticles);
            await cache.RemoveAsync(CacheKeys.AllPublishedArticles);

            return id;
        }

        public async Task<IReadOnlyList<ArticleDto>> GetAllArticlesAsync(bool includeUnpublished)
            => await cache.CacheAsync(
                includeUnpublished ? CacheKeys.AllArticles : CacheKeys.AllPublishedArticles, 
                CacheKeys.TimeToLive, 
                () => articleRepository.FindAsync(includeUnpublished));

        public async Task<ArticleDto> GetArticleByIdAsync(string id)
            => await cache.CacheAsync(
                CacheKeys.Article(id),
                CacheKeys.TimeToLive,
                () => articleRepository.FindByIdAsync(id));

        public async Task SaveArticleAsync(SaveArticleDto model)
        {
            await articleRepository.UpdateAsync(model.Article);
            var oldTags = await tagRepository.FindAsync(model.Article.Id);
            foreach (var tag in model.Tags.Concat(oldTags.Select(x => x.Name)))
            {
                await cache.RemoveAsync(CacheKeys.ArticlesByTag(tag));
                await cache.RemoveAsync(CacheKeys.PublishedArticlesByTag(tag));
            }

            await cache.RemoveAsync(CacheKeys.Article(model.Article.Id));
            await cache.RemoveAsync(CacheKeys.AllArticles);
            await cache.RemoveAsync(CacheKeys.AllPublishedArticles);

            // TODO: referencing TagService here could lead to circular references
            await tagService.UpdateTagsForArticelAsync(model);
        }

        public async Task VisitArticleAsync(string id, string remoteIp, string userAgent)
        {
            await visitorRepository.InsertAsync(id, remoteIp, userAgent);
            await cache.RemoveAsync(CacheKeys.VisitorsByArticleId(id));
        }

        public async Task<long> CountVisitorsAsync(string id)
            => await cache.CacheAsync(
                CacheKeys.VisitorsByArticleId(id), 
                CacheKeys.TimeToLive, 
                () => visitorRepository.CountAsync(id));

        public async Task<IReadOnlyList<ArticleDto>> GetTopTreeArticlesAsync(string excludeId)
        {
            var top5 = await GetTopFiveArticlesAsync();
            return top5.Where(x => x.Id.ToString() != excludeId).Take(3).ToList();
        }
        
        public async Task<IReadOnlyList<ArticleDto>> GetArticlesByTagAsync(string tag, bool includeUnpublished)
        {
            return await cache.CacheAsync(includeUnpublished ? CacheKeys.ArticlesByTag(tag) : CacheKeys.PublishedArticlesByTag(tag), CacheKeys.TimeToLive, async () =>
            {
                var tags = await tagRepository.FindAsync();
                var articles = await GetAllArticlesAsync(includeUnpublished);
                return articles.Where(a => tags.Any(t => t.ArticleId == a.Id && t.Name.ToLower() == tag.ToLower())).ToList();
            });
        }

        public async Task<IReadOnlyList<ArticleWithVisitsDto>> GetTopTreeWithVisitorCountAsync(string excludeId)
            => await WithVisitorCountAsync(await GetTopTreeArticlesAsync(excludeId));

        public async Task<IReadOnlyList<ArticleWithVisitsDto>> GetAllArticlesByTagWithVisitorCountAsync(string tag, bool includeUnpublished)
            => await WithVisitorCountAsync(await GetArticlesByTagAsync(tag, includeUnpublished));

        public async Task<IReadOnlyList<ArticleWithVisitsDto>> GetAllArticlesWithVisitorCountAsync(bool includeUnpublished)
            => await WithVisitorCountAsync(await GetAllArticlesAsync(includeUnpublished));

        private async Task<IReadOnlyList<ArticleDto>> GetTopFiveArticlesAsync()
        {
            // recalculate top 5 articles every day
            return await cache.CacheAsync(CacheKeys.Top5Articles, TimeSpan.FromDays(1), async () =>
            {
                var articles = await articleRepository.FindAsync(includeUnpublished: false);
                var withCount = await Task.WhenAll(articles.Select(async a => new { Article = a, Count = await CountVisitorsAsync(a.Id) }));
                return withCount.OrderByDescending(x => x.Count).Select(x => x.Article).Take(5).ToList();
            });
        }

        private async Task<IReadOnlyList<ArticleWithVisitsDto>> WithVisitorCountAsync(IReadOnlyList<ArticleDto> articles)
            => (await Task.WhenAll(articles.Select(async x => (Article: x, VisitorCount: await CountVisitorsAsync(x.Id))))).Select(x => new ArticleWithVisitsDto { Article = x.Article, VisitorCount = x.VisitorCount }).ToList();
    }
}
