using System;
using AutoMapper;
using NtFreX.Blog.Models;

namespace NtFreX.Blog
{
    public class ApplictionMapperConfig
    {
        public static void ConfigureAutomapper(IMapperConfigurationExpression x) 
        {
            x.CreateMap<CreateCommentDto, CommentModel>()
                .ForMember(m => m.Date, m => m.MapFrom(v => DateTime.UtcNow));
            x.CreateMap<ArticleDto, ArticleModel>();

            x.CreateMap<CommentModel, CommentDto>();
            x.CreateMap<TagModel, TagDto>();
            x.CreateMap<ArticleModel, ArticleDto>();
        }
    }
}
