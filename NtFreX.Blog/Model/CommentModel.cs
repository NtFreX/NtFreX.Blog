using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace NtFreX.Blog.Model
{
    public class CommentModel
    {
        [BsonElement("id", Order = 0)]
        public ObjectId Id { get; set; }

        [BsonElement("article_id", Order = 1)]
        public ObjectId ArticleId { get; set; }

        [BsonElement("date", Order = 2)]
        public DateTime Date { get; set; }

        [BsonElement("user", Order = 3)]
        public string User { get; set; }

        [BsonElement("title", Order = 4)]
        public string Title { get; set; }

        [BsonElement("content", Order = 5)]
        public string Content { get; set; }

        [BsonElement("analysis_prediction", Order = 6)]
        public bool? AnalysisPrediction { get; set; }

        [BsonElement("analysis_probability", Order = 7)]
        public double? AnalysisProbability { get; set; }

        [BsonElement("analysis_sentiment", Order = 8)]
        public string? AnalysisSentiment { get; set; }
    }
}
