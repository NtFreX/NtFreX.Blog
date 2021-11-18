using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MongoDB.Driver;
using NtFreX.Blog.Cache;
using NtFreX.Blog.Configuration;
using NtFreX.Blog.Data;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Services
{
    public class CommentService
    {
        private readonly ICommentRepository commentRepository;
        private readonly IMapper mapper;
        private readonly ApplicationCache cache;

        public CommentService(ICommentRepository commentRepository, IMapper mapper, ApplicationCache cache)
        {
            this.commentRepository = commentRepository;
            this.mapper = mapper;
            this.cache = cache;
        }


        public async Task<IReadOnlyList<CommentDto>> GetCommentsByArticleIdAsync(string id)
        {
            return await cache.CacheAsync(CacheKeys.CommentsByArticleId(id), CacheKeys.TimeToLive, async () =>
            {
                var dbModels = await commentRepository.FindByArticleIdAsync(id);
                return mapper.Map<List<CommentDto>>(dbModels).OrderByDescending(x => x.Date).ToList();
            });            
        }

        public async Task InsertCommentAsync(CreateCommentDto model)
        {
            var activitySource = new ActivitySource(BlogConfiguration.ActivitySourceName);
            using (var sampleActivity = activitySource.StartActivity($"{nameof(CommentService)}.{nameof(InsertCommentAsync)}", ActivityKind.Server))
            {
                var dbModel = mapper.Map<CommentModel>(model);
                dbModel.Date = DateTime.UtcNow;

                await commentRepository.InsertAsync(dbModel);
                await cache.RemoveSaveAsync(CacheKeys.CommentsByArticleId(model.ArticleId.ToString()));
            }
        }
    }
}
