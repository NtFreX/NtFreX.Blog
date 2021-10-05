using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Data
{
    public interface IArticleRepository
    {
        Task<string> InsertArticleAsync();
        Task UpdateAsync(ArticleModel model);
        Task<IReadOnlyList<ArticleModel>> FindAsync(bool includeUnpublished);
        Task<ArticleModel> FindByIdAsync(string id);
        public async Task<IReadOnlyList<ArticleModel>> FindAsync(int page, int size, bool includeUnpublished)
            => (await FindAsync(includeUnpublished)).Skip(page * size).Take(size).ToList();

    }
}
