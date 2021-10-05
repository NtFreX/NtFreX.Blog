using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using NtFreX.Blog.Cache;
using NtFreX.Blog.Data;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Services
{
    public class CommentService
    {
        private readonly ICommentRepository commentRepository;
        private readonly ApplicationCache cache;

        public CommentService(ICommentRepository commentRepository, ApplicationCache cache)
        {
            this.commentRepository = commentRepository;
            this.cache = cache;
        }


        public async Task<IReadOnlyList<CommentDto>> GetCommentsByArticleIdAsync(string id)
        {
            return await cache.CacheAsync(CacheKeys.CommentsByArticleId(id), CacheKeys.TimeToLive, async () =>
            {
                var dbModels = await commentRepository.GetCommentsByArticleIdAsync(id);
                return dbModels.Select(x => x.ToDto()).ToList();
            });            
        }

        public async Task InsertCommentAsync(CreateCommentDto model)
        {
            var dbModel = new CommentModel
            {
                Date = DateTime.UtcNow,
                Content = model.Content,
                ArticleId = model.ArticleId,
                Title = model.Title,
                User = model.User
            };
            await commentRepository.InsertCommentAsync(dbModel);
            await cache.RemoveSaveAsync(CacheKeys.CommentsByArticleId(model.ArticleId.ToString()));
        }
    }
}
