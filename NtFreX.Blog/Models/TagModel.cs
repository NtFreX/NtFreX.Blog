using Dapper.Contrib.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NtFreX.Blog.Models
{
    [Table("tag")]
    public class TagModel
    {
        [ExplicitKey]
        [BsonElement("id", Order = 0)]
        public string Id { get; set; }

        [BsonElement("article_id", Order = 1)]
        public string ArticleId { get; set; }

        [BsonElement("name", Order = 2)]
        public string Name { get; set; }
    }
}
