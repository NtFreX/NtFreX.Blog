using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MongoDB.Bson;
using MongoDB.Driver;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Data.MongoDb
{
    public class MongoDbArticleRepository : MongoDbRepository<Models.ArticleModel, ArticleModel>, IArticleRepository
    {
        public MongoDbArticleRepository(MongoDatabase database, IMapper mapper)
            : base(database.Blog.GetCollection<Models.ArticleModel>("article"), mapper)
        { }

        public override async Task UpdateAsync(ArticleModel model)
        {
            var objectId = new ObjectId(model.Id);
            await Collection.UpdateOneAsync(
                Builders<Models.ArticleModel>.Filter.Eq(d => d.Id, objectId),
                Builders<Models.ArticleModel>.Update
                    .Set(d => d.Title, model.Title)
                    .Set(d => d.Subtitle, model.Subtitle)
                    .Set(d => d.Date, model.Date)
                    .Set(d => d.Published, model.Published)
                    .Set(d => d.Content, model.Content));
        }
    }
}
