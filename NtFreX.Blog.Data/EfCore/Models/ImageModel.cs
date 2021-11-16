using Dapper.Contrib.Extensions;

namespace NtFreX.Blog.Data.EfCore.Models
{
    [Table("image")]
    public class ImageModel : IEfCoreDbModel
    {
        [ExplicitKey]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
    }
}