using System;
using AutoMapper;
using NtFreX.Blog.Models;
using NtFreX.Blog.Data.Models;

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
            x.CreateMap<ImageModel, ImageDto>();
            x.CreateMap<VisitorModel, VisitorDto>();
        }
    }
}
