using System;

namespace NtFreX.Blog.Models
{
    public class CommentModel
    {
        public string Id { get; set; }
        public string ArticleId { get; set; }
        public DateTime Date { get; set; }
        public string User { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool? AnalysisPrediction { get; set; }
        public double? AnalysisProbability { get; set; }
        public string AnalysisSentiment { get; set; }
    }
}
