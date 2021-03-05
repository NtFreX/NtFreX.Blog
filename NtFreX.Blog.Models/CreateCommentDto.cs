namespace NtFreX.Blog.Models
{
    public class CreateCommentDto
    {
        public string ArticleId { get; set; }
        public string User { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
