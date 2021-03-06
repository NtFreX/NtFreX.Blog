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
    public class TagService
    {
        private readonly TagRepository tagRepository;
        private readonly ArticleRepository articleRepository;
        private readonly IDistributedCache cache;

        public TagService(TagRepository tagRepository, ArticleRepository articleRepository, IDistributedCache cache)
        {
            this.tagRepository = tagRepository;
            this.articleRepository = articleRepository;
            this.cache = cache;
        }

        public async Task UpdateTagsForArticelAsync(SaveArticleDto model)
        {
            await tagRepository.UpdateTagsForArticle(model.Tags, model.Article.Id);

            await cache.RemoveAsync(CacheKeys.AllDistinctTags);
            await cache.RemoveAsync(CacheKeys.AllPublishedTags);
            await cache.RemoveAsync(CacheKeys.AllDistinctPublishedTags);
            await cache.RemoveAsync(CacheKeys.AllTags);
            await cache.RemoveAsync(CacheKeys.TagsByArticleId(model.Article.Id));
        }


        public async Task<IReadOnlyList<TagDto>> GetAllTagsAsync(bool includeUnpublished)
        {
            return await cache.CacheAsync(includeUnpublished ? CacheKeys.AllTags : CacheKeys.AllPublishedTags, CacheKeys.TimeToLive, async () =>
            {
                var tags = await tagRepository.FindAsync();
                if (includeUnpublished)
                    return tags;

                var articles = await articleRepository.FindAsync(includeUnpublished);
                return tags.Where(x => articles.Any(article => article.Id == x.ArticleId)).ToList();
            });
        }

        public async Task<IReadOnlyList<TagDto>> GetTagsByArticleIdAsync(string articleId)
            => await cache.CacheAsync(
                CacheKeys.TagsByArticleId(articleId), 
                CacheKeys.TimeToLive, 
                () => tagRepository.FindAsync(articleId));

        public async Task<IReadOnlyList<string>> GetAllDistinctTagsAsync(bool includeUnpublished)
        {
            return await cache.CacheAsync(includeUnpublished ? CacheKeys.AllDistinctTags : CacheKeys.AllDistinctPublishedTags, CacheKeys.TimeToLive, async () =>
            {
                var all = await GetAllTagsAsync(includeUnpublished);
                return all.GroupBy(d => d.Name).OrderByDescending(d => d.Count()).Select(d => d.Key).ToList();
            });
        }
    }
}
