using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using NtFreX.Blog.Cache;
using NtFreX.Blog.Data;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Services
{
    public class TagService
    {
        private readonly ITagRepository tagRepository;
        private readonly IArticleRepository articleRepository;
        private readonly ApplicationCache cache;

        public TagService(ITagRepository tagRepository, IArticleRepository articleRepository, ApplicationCache cache)
        {
            this.tagRepository = tagRepository;
            this.articleRepository = articleRepository;
            this.cache = cache;
        }

        public async Task UpdateTagsForArticelAsync(SaveArticleDto model)
        {
            await tagRepository.UpdateTagsForArticle(model.Tags, model.Article.Id);

            await cache.RemoveSaveAsync(CacheKeys.AllDistinctTags);
            await cache.RemoveSaveAsync(CacheKeys.AllPublishedTags);
            await cache.RemoveSaveAsync(CacheKeys.AllDistinctPublishedTags);
            await cache.RemoveSaveAsync(CacheKeys.AllTags);
            await cache.RemoveSaveAsync(CacheKeys.TagsByArticleId(model.Article.Id));
        }


        public async Task<IReadOnlyList<TagDto>> GetAllTagsAsync(bool includeUnpublished)
        {
            return await cache.CacheAsync(includeUnpublished ? CacheKeys.AllTags : CacheKeys.AllPublishedTags, CacheKeys.TimeToLive, async () =>
            {
                var tags = await tagRepository.FindAsync();
                if (includeUnpublished)
                    return tags.Select(x => x.ToDto()).ToList();

                var articles = await articleRepository.FindAsync(includeUnpublished);
                return tags.Where(x => articles.Any(article => article.Id.ToString() == x.ArticleId)).Select(x => x.ToDto()).ToList();
            });
        }

        public async Task<IReadOnlyList<TagDto>> GetTagsByArticleIdAsync(string articleId)
            => await cache.CacheAsync(
                CacheKeys.TagsByArticleId(articleId),
                CacheKeys.TimeToLive,
                async () => (await tagRepository.FindAsync(articleId)).Select(x => x.ToDto()).ToList());

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
