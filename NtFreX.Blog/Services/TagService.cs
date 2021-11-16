using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
        private readonly IMapper mapper;
        private readonly ApplicationCache cache;

        public TagService(ITagRepository tagRepository, IArticleRepository articleRepository, IMapper mapper, ApplicationCache cache)
        {
            this.tagRepository = tagRepository;
            this.articleRepository = articleRepository;
            this.mapper = mapper;
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
                    return mapper.Map<List<TagDto>>(tags);

                var articles = await articleRepository.FindAsync();
                var published = mapper.Map<List<ArticleDto>>(articles).Where(x => x.IsPublished()).ToList();

                var publishedTags = tags.Where(x => published.Any(article => article.Id.ToString() == x.ArticleId)).ToList();
                return mapper.Map<List<TagDto>>(publishedTags);
            });
        }

        public async Task<IReadOnlyList<TagDto>> GetTagsByArticleIdAsync(string articleId)
            => await cache.CacheAsync(
                CacheKeys.TagsByArticleId(articleId),
                CacheKeys.TimeToLive,
                async () => mapper.Map<List<TagDto>>(await tagRepository.FindByArticleIdAsync(articleId)));

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
