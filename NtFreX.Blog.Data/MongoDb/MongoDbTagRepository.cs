using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MongoDB.Driver;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Data.MongoDb
{
    public class MongoDbTagRepository : MongoDbRepository<Models.TagModel, TagModel>, ITagRepository
    {
        private readonly IMapper mapper;

        public MongoDbTagRepository(MongoConnectionFactory database, IMapper mapper)
            : base(database, database.Blog.GetCollection<Models.TagModel>("tag"), mapper)
        {
            this.mapper = mapper;
        }

        public async Task<IReadOnlyList<TagModel>> FindByArticleIdAsync(string articleId)
        {
            var dbModels = await Collection.Find(d => d.ArticleId == articleId).ToListAsync();
            return mapper.Map<List<TagModel>>(dbModels);
        }

        public override Task UpdateAsync(TagModel model)
            => throw new NotSupportedException("Updating a tag is not supported");

        public async Task UpdateTagsForArticle(string[] newTags, string articleId)
        {
            await Collection.DeleteManyAsync(Database.Session, Builders<Models.TagModel>.Filter.Eq(d => d.ArticleId, articleId));

            if (newTags.Any())
            {
                await Collection.InsertManyAsync(Database.Session, newTags.Select(x => new Models.TagModel { ArticleId = articleId, Name = x }));
            }
        }
    }
}
