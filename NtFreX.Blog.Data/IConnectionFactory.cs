using System.Threading.Tasks;

namespace NtFreX.Blog.Data
{
    public interface IConnectionFactory
    {
        Task BeginTransactionAsync();
        Task CommitTansactionAsync();
        Task RollbackTansactionAsync();
    }
}
