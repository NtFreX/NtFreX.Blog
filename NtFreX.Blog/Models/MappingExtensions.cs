namespace NtFreX.Blog.Models
{
    public static class MappingExtensions
    {
        public static ArticleDto ToDto(this ArticleModel x)
            => new ArticleDto
            {
                Content = x.Content,
                Date = x.Date,
                Id = x.Id.ToString(),
                Published = x.Published,
                Subtitle = x.Subtitle,
                Title = x.Title
            };

        public static CommentDto ToDto(this CommentModel x)
            => new CommentDto
            {
                ArticleId = x.ArticleId.ToString(),
                Content = x.Content,
                Title = x.Title,
                User = x.User
            };

        public static TagDto ToDto(this TagModel x)
            => new TagDto
            {
                ArticleId = x.ArticleId.ToString(),
                Name = x.Name
            };
    }
}
