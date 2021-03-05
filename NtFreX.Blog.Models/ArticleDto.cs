using System;

namespace NtFreX.Blog.Models
{
    public class ArticleDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Content { get; set; }
        public bool Published { get; set; }
        public DateTime Date { get; set; }

        public bool IsPublished() => Published && Date <= DateTime.UtcNow;
    }
}
