using System.Collections.Generic;
using System.Threading.Tasks;
using NtFreX.Blog.Data.Models;

namespace NtFreX.Blog.Data
{
    public interface ITagRepository : IRepository<TagModel>
    {
        Task<IReadOnlyList<TagModel>> FindByArticleIdAsync(string articleId);
        Task UpdateTagsForArticle(string[] newTags, string articleId);
    }
}
