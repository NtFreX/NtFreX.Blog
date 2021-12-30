using System.Threading.Tasks;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Data
{
    public interface IImageRepository : IRepository<ImageModel>
    {
        Task<ImageModel> FindByNameAsync(string name);
    }
}
