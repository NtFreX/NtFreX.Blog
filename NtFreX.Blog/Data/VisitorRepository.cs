using System.Net;
using System.Threading.Tasks;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Data
{
    public abstract class VisitorRepository
    {
        public abstract Task InsertAsync(VisitorModel model);
        public abstract Task<long> CountAsync(string id);

        public bool DoCountVisitor(VisitorModel d) => string.IsNullOrEmpty(d.RemoteIp) || !IPAddress.IsLoopback(IPAddress.Parse(d.RemoteIp));
    }
}
