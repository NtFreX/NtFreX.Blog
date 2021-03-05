namespace NtFreX.Blog.Models
{
    public class SaveArticleDto
    {
        public ArticleDto Article { get; set; }
        public string[] Tags { get; set; }
    }
}
