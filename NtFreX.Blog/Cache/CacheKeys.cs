using System;

namespace NtFreX.Blog.Cache
{
    public static class CacheKeys
    {
        public static (string Name, TimeSpan TimeToLive) AllTags = (nameof(AllTags), TimeSpan.FromDays(7));
        public static (string Name, TimeSpan TimeToLive) AllPublishedTags = (nameof(AllPublishedTags), TimeSpan.FromDays(7));
        public static (string Name, TimeSpan TimeToLive) AllDistinctTags = (nameof(AllDistinctTags), TimeSpan.FromDays(7));
        public static (string Name, TimeSpan TimeToLive) AllDistinctPublishedTags = (nameof(AllDistinctPublishedTags), TimeSpan.FromDays(7));
        public static (Func<string, string> Name, TimeSpan TimeToLive) TagsByArticleId => ((string articleId) => $"TagsByArticleId{articleId}", TimeSpan.FromDays(7));
        public static (Func<string, string> Name, TimeSpan TimeToLive) CommentsByArticleId => ((string articleId) => $"CommentsByArticleId{articleId}", TimeSpan.FromDays(7));
        public static (Func<string, string> Name, TimeSpan TimeToLive) VisitorsByArticleId => ((string articleId) => $"VisitorsByArticleId{articleId}", TimeSpan.FromDays(7));
        public static (Func<string, string> Name, TimeSpan TimeToLive) Article => ((string articleId) => $"Article{articleId}", TimeSpan.FromDays(7));
        public static (string Name, TimeSpan TimeToLive) AllArticles = (nameof(AllArticles), TimeSpan.FromDays(7));
        public static (string Name, TimeSpan TimeToLive) AllPublishedArticles = (nameof(AllPublishedArticles), TimeSpan.FromDays(7));
        public static (string Name, TimeSpan TimeToLive) Top5Articles = (nameof(Top5Articles), TimeSpan.FromDays(1));
        public static (Func<string, string> Name, TimeSpan TimeToLive) ArticlesByTag = ((string tag) => $"ArticlesByTag{tag}", TimeSpan.FromDays(7));
        public static (Func<string, string> Name, TimeSpan TimeToLive) PublishedArticlesByTag = ((string tag) => $"PublishedArticlesByTag{tag}", TimeSpan.FromDays(7));
        public static (string Name, TimeSpan TimeToLive) AllImages = (nameof(AllImages), TimeSpan.FromDays(7));
        public static (Func<string, string> Name, TimeSpan TimeToLive) Image = ((string name) => $"Image{name}", TimeSpan.FromDays(7));
        public static (Func<string, string> Name, TimeSpan TimeToLive) FailedLoginRequests = ((string username) => $"FailedLoginRequests{username}", TimeSpan.FromHours(24));
        public static (Func<string, string> Name, TimeSpan TimeToLive) TwoFactorSession = ((string session) => $"TwoFactorSession{session}", TimeSpan.FromMinutes(5));
    }
}
