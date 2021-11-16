using Dapper.Contrib.Extensions;
using System;

namespace NtFreX.Blog.Data.EfCore.Models
{
    [Table("article")]
    public class ArticleModel : IEfCoreDbModel
    {
        [ExplicitKey]
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Content { get; set; }
        public bool Published { get; set; }
    }
}
