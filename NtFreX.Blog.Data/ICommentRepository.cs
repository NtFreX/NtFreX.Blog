using System.Collections.Generic;
using System.Threading.Tasks;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Data
{
    public interface ICommentRepository : IRepository<CommentModel>
    {
        Task<IReadOnlyList<CommentModel>> FindByArticleIdAsync(string articleId);
    }
}
