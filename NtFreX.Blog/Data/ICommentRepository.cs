using System.Collections.Generic;
using System.Threading.Tasks;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Data
{
    public interface ICommentRepository
    {
        Task<IReadOnlyList<CommentModel>> GetCommentsByArticleIdAsync(string id);
        Task InsertCommentAsync(CommentModel model);
    }
}
