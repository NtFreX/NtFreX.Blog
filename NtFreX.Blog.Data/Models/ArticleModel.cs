using System;

namespace NtFreX.Blog.Data.Models
{
    public class ArticleModel
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Content { get; set; }
        public bool Published { get; set; }
    }
}
