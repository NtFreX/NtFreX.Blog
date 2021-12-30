using AutoMapper;
using MongoDB.Bson;
using NtFreX.Blog.Data.Models;

namespace NtFreX.Blog.Data
{
    public class ApplictionMapperConfig
    {
        public static void ConfigureAutomapper(IMapperConfigurationExpression x)
        {
            x.CreateMap<EfCore.Models.CommentModel, CommentModel>();
            x.CreateMap<EfCore.Models.ImageModel, ImageModel>();
            x.CreateMap<EfCore.Models.ArticleModel, ArticleModel>();
            x.CreateMap<EfCore.Models.TagModel, TagModel>();
            x.CreateMap<EfCore.Models.VisitorModel, VisitorModel>();
            x.CreateMap<CommentModel, EfCore.Models.CommentModel>();
            x.CreateMap<ImageModel, EfCore.Models.ImageModel>();
            x.CreateMap<ArticleModel, EfCore.Models.ArticleModel>();
            x.CreateMap<TagModel, EfCore.Models.TagModel>();
            x.CreateMap<VisitorModel, EfCore.Models.VisitorModel>();

            x.CreateMap<MongoDb.Models.CommentModel, CommentModel>()
                .ForMember(x => x.Id, x => x.MapFrom(v => v.Id.ToString()));
            x.CreateMap<MongoDb.Models.ImageModel, ImageModel>()
                .ForMember(x => x.Id, x => x.MapFrom(v => v.Id.ToString()));
            x.CreateMap<MongoDb.Models.ArticleModel, ArticleModel>()
                .ForMember(x => x.Id, x => x.MapFrom(v => v.Id.ToString()));
            x.CreateMap<MongoDb.Models.TagModel, TagModel>()
                .ForMember(x => x.Id, x => x.MapFrom(v => v.Id.ToString()));
            x.CreateMap<MongoDb.Models.VisitorModel, VisitorModel>()
                .ForMember(x => x.Id, x => x.MapFrom(v => v.Id.ToString()));
            x.CreateMap<CommentModel, MongoDb.Models.CommentModel>()
                .ForMember(x => x.Id, x => x.MapFrom(v => new ObjectId(v.Id)));
            x.CreateMap<ImageModel, MongoDb.Models.ImageModel>()
                .ForMember(x => x.Id, x => x.MapFrom(v => new ObjectId(v.Id)));
            x.CreateMap<ArticleModel, MongoDb.Models.ArticleModel>()
                .ForMember(x => x.Id, x => x.MapFrom(v => new ObjectId(v.Id)));
            x.CreateMap<TagModel, MongoDb.Models.TagModel>()
                .ForMember(x => x.Id, x => x.MapFrom(v => new ObjectId(v.Id)));
            x.CreateMap<VisitorModel, MongoDb.Models.VisitorModel>()
                .ForMember(x => x.Id, x => x.MapFrom(v => new ObjectId(v.Id)));
        }
    }
}
