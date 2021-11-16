using Dapper.Contrib.Extensions;

namespace NtFreX.Blog.Data.EfCore.Models
{
    [Table("tag")]
    public class TagModel : IEfCoreDbModel
    {
        [ExplicitKey]
        public string Id { get; set; }
        public string ArticleId { get; set; }
        public string Name { get; set; }
    }
}
