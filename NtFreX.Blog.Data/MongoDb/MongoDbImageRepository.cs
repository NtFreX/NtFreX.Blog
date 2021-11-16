using System.Threading.Tasks;
using AutoMapper;
using MongoDB.Driver;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Data.MongoDb
{
    public class MongoDbImageRepository : MongoDbRepository<Models.ImageModel, ImageModel>, IImageRepository
    {
        private readonly IMapper mapper;

        public MongoDbImageRepository(MongoDatabase database, IMapper mapper)
            : base(database.Blog.GetCollection<Models.ImageModel>("image"), mapper)
        {
            this.mapper = mapper;
        }

        public async Task<ImageModel> FindByName(string name)
        { 
            var dbModel = await Collection.Find(d => d.Name == name).FirstAsync();
            return mapper.Map<ImageModel>(dbModel);
        }

        public override async Task UpdateAsync(ImageModel model)
        {
            await Collection.FindOneAndDeleteAsync(Builders<Models.ImageModel>.Filter.Eq(x => x.Name, model.Name));
            await InsertAsync(model);
        }
    }
}
