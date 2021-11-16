using System.Net;
using System.Threading.Tasks;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Data
{
    public interface IVisitorRepository : IRepository<VisitorModel>
    {
        Task<long> CountByArticleIdAsync(string articleId);

        public static bool ShouldCountVisitor(VisitorModel d) => string.IsNullOrEmpty(d.RemoteIp) || !IPAddress.IsLoopback(IPAddress.Parse(d.RemoteIp));
    }
}
