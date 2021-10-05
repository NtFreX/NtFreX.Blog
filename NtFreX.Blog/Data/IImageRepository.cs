using System.Threading.Tasks;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Data
{
    public interface IImageRepository
    {
        Task<ImageModel> FindByName(string name);
        Task InsertOrUpdate(ImageModel image);
    }
}
