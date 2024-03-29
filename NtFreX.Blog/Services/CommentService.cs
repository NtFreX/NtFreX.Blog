﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using MongoDB.Driver;
using NtFreX.Blog.Cache;
using NtFreX.Blog.Configuration;
using NtFreX.Blog.Data;
using NtFreX.Blog.Messaging;
using NtFreX.Blog.Models;
using NtFreX.Blog.Data.Models;

namespace NtFreX.Blog.Services
{
    public class CommentService
    {
        private readonly ApplicationContextActivityDecorator traceActivityDecorator;
        private readonly ICommentRepository commentRepository;
        private readonly IMapper mapper;
        private readonly ApplicationCache cache;
        private readonly IMessageBus messageBus;

        private static readonly Counter<int> CommmentCreatedCounter = Program.Meter.CreateCounter<int>($"CommentCreated", description: "The number of comments created");
        
        public CommentService(ApplicationContextActivityDecorator traceActivityDecorator, ICommentRepository commentRepository, IMapper mapper, ApplicationCache cache, IMessageBus messageBus)
        {
            this.traceActivityDecorator = traceActivityDecorator;
            this.commentRepository = commentRepository;
            this.mapper = mapper;
            this.cache = cache;
            this.messageBus = messageBus;
        }

        public async Task<IReadOnlyList<CommentDto>> GetAllCommentsAsync()
        {
            using var activity = traceActivityDecorator.StartActivity();
            var cacheKey = CacheKeys.AllComments;

            return await cache.CacheAsync(cacheKey.Name, cacheKey.TimeToLive, async () =>
            {
                var dbModels = await commentRepository.FindAsync();
                return mapper.Map<List<CommentDto>>(dbModels).ToList();
            });
        }

        public async Task<IReadOnlyList<CommentDto>> GetCommentsByArticleIdAsync(string id)
        {
            using var activity = traceActivityDecorator.StartActivity();
            var cacheKey = CacheKeys.CommentsByArticleId;

            return await cache.CacheAsync(cacheKey.Name(id), cacheKey.TimeToLive, async () =>
            {
                var dbModels = await commentRepository.FindByArticleIdAsync(id);
                return mapper.Map<List<CommentDto>>(dbModels).OrderByDescending(x => x.Date).ToList();
            });            
        }

        public async Task InsertCommentAsync(CreateCommentDto model)
        {
            using var activity = traceActivityDecorator.StartActivity();

            var dbModel = mapper.Map<CommentModel>(model);
            dbModel.Date = DateTime.UtcNow;

            await commentRepository.InsertAsync(dbModel);
            await cache.RemoveSaveAsync(CacheKeys.AllComments.Name);
            await cache.RemoveSaveAsync(CacheKeys.CommentsByArticleId.Name(model.ArticleId));

            var tags = new[] {
                    new KeyValuePair<string, object>("user", model.User)
            }.Concat(MetricTags.GetDefaultTags()).ToArray();
            CommmentCreatedCounter.Add(1, tags);

            await messageBus.SendMessageAsync("ntfrex.blog.comments", JsonSerializer.Serialize(model));
        }
    }
}
