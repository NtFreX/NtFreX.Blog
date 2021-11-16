using System.Collections.Generic;
using System.Threading.Tasks;

namespace NtFreX.Blog.Data
{
    public interface IRepository<TModel>
    {
        Task<string> InsertAsync(TModel model);
        Task UpdateAsync(TModel model);
        Task<TModel> FindByIdAsync(string id);
        Task<IReadOnlyList<TModel>> FindAsync();
        Task InsertOrUpdate(TModel model);
    }
}
