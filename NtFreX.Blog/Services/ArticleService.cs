using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MongoDB.Driver;
using NtFreX.Blog.Cache;
using NtFreX.Blog.Configuration;
using NtFreX.Blog.Data;
using NtFreX.Blog.Logging;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Services
{
    public class ArticleService
    {
        private readonly TraceActivityDecorator traceActivityDecorator;
        private readonly IArticleRepository articleRepository;
        private readonly ITagRepository tagRepository;
        private readonly TagService tagService;
        private readonly IVisitorRepository visitorRepository;
        private readonly IMapper mapper;
        private readonly ApplicationCache cache;

        public ArticleService(TraceActivityDecorator traceActivityDecorator, IArticleRepository articleRepository, ITagRepository tagRepository, TagService tagService, IVisitorRepository visitorRepository, IMapper mapper, ApplicationCache cache) 
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
            
            await cache.RemoveSaveAsync(CacheKeys.AllArticles);
            await cache.RemoveSaveAsync(CacheKeys.AllPublishedArticles);

            return id;
        }

        public async Task<IReadOnlyList<ArticleDto>> GetAllArticlesAsync(bool includeUnpublished)
        {
            var activitySource = new ActivitySource(BlogConfiguration.ActivitySourceName);
            using (var activity = activitySource.StartActivity($"{nameof(ArticleService)}.{nameof(GetAllArticlesAsync)}", ActivityKind.Server))
            {
                traceActivityDecorator.Decorate(activity);

                return await cache.CacheAsync(
                    includeUnpublished ? CacheKeys.AllArticles : CacheKeys.AllPublishedArticles,
                    CacheKeys.TimeToLive,
                    () => FindAsync(includeUnpublished));
            }
        }

        public async Task<ArticleDto> GetArticleByIdAsync(string id)
        {
            var activitySource = new ActivitySource(BlogConfiguration.ActivitySourceName);
            using (var activity = activitySource.StartActivity($"{nameof(ArticleService)}.{nameof(GetArticleByIdAsync)}", ActivityKind.Server))
            {
                traceActivityDecorator.Decorate(activity);

                return await cache.CacheAsync(
                    CacheKeys.Article(id),
                    CacheKeys.TimeToLive,
                    async () => mapper.Map<ArticleDto>(await articleRepository.FindByIdAsync(id)));
            }
        }

        public async Task SaveArticleAsync(SaveArticleDto model)
        {
            var activitySource = new ActivitySource(BlogConfiguration.ActivitySourceName);
            using (var activity = activitySource.StartActivity($"{nameof(ArticleService)}.{nameof(SaveArticleAsync)}", ActivityKind.Server))
            {
                traceActivityDecorator.Decorate(activity);

                await articleRepository.UpdateAsync(mapper.Map<ArticleModel>(model.Article));
                var oldTags = await tagRepository.FindByArticleIdAsync(model.Article.Id);
                foreach (var tag in model.Tags.Concat(oldTags.Select(x => x.Name)))
                {
                    await cache.RemoveSaveAsync(CacheKeys.ArticlesByTag(tag));
                    await cache.RemoveSaveAsync(CacheKeys.PublishedArticlesByTag(tag));
                }

                await cache.RemoveSaveAsync(CacheKeys.Article(model.Article.Id));
                await cache.RemoveSaveAsync(CacheKeys.AllArticles);
                await cache.RemoveSaveAsync(CacheKeys.AllPublishedArticles);

                // Careful: referencing TagService here could lead to circular references
                await tagService.UpdateTagsForArticelAsync(model);
            }
        }

        public async Task VisitArticleAsync(string id, string remoteIp, string userAgent)
        {
            var model = new VisitorModel
            {
                Date = DateTime.Now,
                RemoteIp = remoteIp,
                Article = id,
                UserAgent = userAgent
            };
            await visitorRepository.InsertAsync(model);
            await cache.RemoveSaveAsync(CacheKeys.VisitorsByArticleId(id));
        }

        public async Task<long> CountVisitorsAsync(string id)
            => await cache.CacheAsync(
                CacheKeys.VisitorsByArticleId(id), 
                CacheKeys.TimeToLive, 
                () => visitorRepository.CountByArticleIdAsync(id));

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
                var articles = await FindAsync(includeUnpublished: false);
                var withCount = await Task.WhenAll(articles.Select(async a => new { Article = a, Count = await CountVisitorsAsync(a.Id.ToString()) }));
                return withCount.OrderByDescending(x => x.Count).Select(x => x.Article).Take(5).ToList();
            });
        }

        private async Task<IReadOnlyList<ArticleWithVisitsDto>> WithVisitorCountAsync(IReadOnlyList<ArticleDto> articles)
            => await Task.WhenAll(articles.Select(async x => new ArticleWithVisitsDto { Article = x, VisitorCount = await CountVisitorsAsync(x.Id) }).ToList());
    }
}
