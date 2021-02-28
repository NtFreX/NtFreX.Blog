using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Bson;
using MongoDB.Driver;
using NtFreX.Blog.Auth;
using NtFreX.Blog.Cache;
using NtFreX.Blog.Data;
using NtFreX.Blog.Model;

namespace NtFreX.Blog.Services
{
    public class ArticleService
    {
        private readonly IMongoCollection<ArticleModel> article;
        private readonly IMongoCollection<VisitorModel> visitor;
        private readonly TagService tagService;
        private readonly IDistributedCache cache;
        private readonly AuthorizationManager authorizationManager;
        private readonly IHttpContextAccessor httpContextAccessor;

        public ArticleService(Database database, TagService tagService, IDistributedCache cache, AuthorizationManager authorizationManager, IHttpContextAccessor httpContextAccessor) 
        {
            article = database.Blog.GetCollection<ArticleModel>("article");
            visitor = database.Blog.GetCollection<VisitorModel>("visitor");
            this.tagService = tagService;
            this.cache = cache;
            this.authorizationManager = authorizationManager;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task VisitArticleAsync(string id)
        {
            var context = httpContextAccessor.HttpContext;
            var model = new VisitorModel
            {
                Date = DateTime.Now,
                RemoteIp = context.Connection.RemoteIpAddress.ToString(),
                Article = id,
                UserAgent = context.Request.Headers["User-Agent"]
            };
            await visitor.InsertOneAsync(model);

            await cache.RemoveAsync(CacheKeys.VisitorsByArticleId(id));
        }

        public async Task<long> CountVisitorsAsync(string id)
        {
            return await cache.CacheAsync(CacheKeys.VisitorsByArticleId(id), CacheKeys.TimeToLive, async () =>
            {
                var items = await visitor.Find(Builders<VisitorModel>.Filter.Eq(d => d.Article, id)).ToListAsync();
                return items.Count(d => !IPAddress.IsLoopback(IPAddress.Parse(d.RemoteIp)));
            });
        }

        public async Task<IReadOnlyList<ArticleModel>> GetTopTreeArticlesAsync(string excludeId)
        {
            var top5 = await GetTopFiveArticlesAsync();
            return top5.Where(x => x.Id.ToString() != excludeId).Take(3).ToList();
        }

        public async Task<IReadOnlyList<ArticleModel>> GetTopFiveArticlesAsync()
        {
            // recalculate top 5 articles every day
            return await cache.CacheAsync(CacheKeys.Top5Articles, TimeSpan.FromDays(1), async () =>
            {
                var articles = await GetAllArticlesAsync(false);
                var withCount = await Task.WhenAll(articles.Select(async a => new { Article = a, Count = await CountVisitorsAsync(a.Id.ToString()) }));
                return withCount.OrderByDescending(x => x.Count).Select(x => x.Article).Take(5).ToList();
            });
        }

        public async Task<string> CreateArticleAsync()
        {
            var model = new ArticleModel();
            await article.InsertOneAsync(model);

            await cache.RemoveAsync(CacheKeys.AllArticles);
            await cache.RemoveAsync(CacheKeys.AllPublishedArticles);

            return model.Id.ToString();
        }
        
        public async Task<IReadOnlyList<ArticleModel>> GetArticlesByTagAsync(string tag)
        {
            return await cache.CacheAsync(CacheKeys.ArticlesByTag(tag), CacheKeys.TimeToLive, async () =>
            {
                var tags = await tagService.GetAllTagsAsync();
                // do not load unpublished articles so we do not have to distinct the cache between the admin view and the non admin view
                var articles = await GetAllArticlesAsync(false);
                return articles.Where(a => tags.Any(t => t.ArticleId == a.Id && t.Name.ToLower() == tag.ToLower())).ToList();
            });
        }

        public async Task<IReadOnlyList<ArticleModel>> GetAllArticlesAsync(bool includeUnpublished)
        {
            if (!authorizationManager.IsAdmin() && includeUnpublished)
                throw new UnauthorizedAccessException();

            return await cache.CacheAsync(includeUnpublished ? CacheKeys.AllArticles : CacheKeys.AllPublishedArticles, CacheKeys.TimeToLive, async () =>
            {
                var items = await article.Find(_ => true).ToListAsync();
                return items.Where(d => includeUnpublished || IsPublished(d)).OrderByDescending(d => d.Date).ToList();
            });            
        }

        public async Task<ArticleModel> GetArticleByIdAsync(string id)
        {
            return await cache.CacheAsync(CacheKeys.Article(id), CacheKeys.TimeToLive, async () =>
            {
                var objectId = new ObjectId(id);
                var item = await article.Find(d => d.Id == objectId).FirstAsync();
                if (!IsPublished(item) && !authorizationManager.IsAdmin())
                {
                    throw new UnauthorizedAccessException();
                }

                return item;
            });
        }

        public async Task SaveArticleAsync(ArticleModel model, string[] tags)
        {
            if (!authorizationManager.IsAdmin())
                throw new UnauthorizedAccessException();

            await article.UpdateOneAsync(
                Builders<ArticleModel>.Filter.Eq(d => d.Id, model.Id),
                Builders<ArticleModel>.Update
                    .Set(d => d.Title, model.Title)
                    .Set(d => d.Subtitle, model.Subtitle)
                    .Set(d => d.Date, model.Date)
                    .Set(d => d.Published, model.Published)
                    .Set(d => d.Content, model.Content));

            await tagService.UpdateTagsForArticle(tags, model.Id);

            await cache.RemoveAsync(CacheKeys.Article(model.Id.ToString()));
            await cache.RemoveAsync(CacheKeys.AllArticles);
            await cache.RemoveAsync(CacheKeys.AllPublishedArticles);
        }

        public static bool IsPublished(ArticleModel article) => article.Published && article.Date <= DateTime.UtcNow;
    }
}
