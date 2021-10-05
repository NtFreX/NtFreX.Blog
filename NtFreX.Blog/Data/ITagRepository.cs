using System.Collections.Generic;
using System.Threading.Tasks;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Data
{
    public interface ITagRepository
    {
        Task<IReadOnlyList<TagModel>> FindAsync();
        Task<IReadOnlyList<TagModel>> FindAsync(string articleId);
        Task UpdateTagsForArticle(string[] newTags, string articleId);
    }
}
