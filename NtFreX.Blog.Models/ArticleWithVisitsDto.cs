namespace NtFreX.Blog.Models
{
    public class ArticleWithVisitsDto
    {
        public ArticleDto Article { get; set; }
        public long VisitorCount { get; set; }
    }
}
