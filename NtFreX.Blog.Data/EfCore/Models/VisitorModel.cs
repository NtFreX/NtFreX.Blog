using Dapper.Contrib.Extensions;
using System;

namespace NtFreX.Blog.Data.EfCore.Models
{
    [Table("visitor")]
    public class VisitorModel : IEfCoreDbModel
    {
        [ExplicitKey]
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public string UserAgent { get; set; }
        public string Article { get; set; }
        public string RemoteIp { get; set; }
    }
}