using System;

namespace NtFreX.Blog.Models
{
    public class CommentDto
    {
        public DateTime Date { get; set; }
        public string ArticleId { get; set; }
        public string User { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
