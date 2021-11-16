using System;

namespace NtFreX.Blog.Cache
{
    public static class CacheKeys
    {
        public static string AllTags = nameof(AllTags);
        public static string AllPublishedTags = nameof(AllPublishedTags);
        public static string AllDistinctTags = nameof(AllDistinctTags);
        public static string AllDistinctPublishedTags = nameof(AllDistinctPublishedTags);
        public static string TagsByArticleId(string articleId) => $"TagsByArticleId{articleId}";
        public static string CommentsByArticleId(string articleId) => $"CommentsByArticleId{articleId}";
        public static string VisitorsByArticleId(string articleId) => $"VisitorsByArticleId{articleId}";
        public static string Article(string articleId) => $"Article{articleId}";
        public static string AllArticles = nameof(AllArticles);
        public static string AllPublishedArticles = nameof(AllPublishedArticles);
        public static string Top5Articles = nameof(Top5Articles);
        public static string ArticlesByTag(string tag) => $"ArticlesByTag{tag}";
        public static string PublishedArticlesByTag(string tag) => $"PublishedArticlesByTag{tag}";
        public static string Image(string name) => $"Image{name}";
        public static string FailedLoginRequests(string username) => $"FailedLoginRequests{username}";

        public static TimeSpan TimeToLive = TimeSpan.FromDays(7);
    }
}
