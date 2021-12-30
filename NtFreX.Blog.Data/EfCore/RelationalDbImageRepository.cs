using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Dapper.Contrib.Extensions;
using MySql.Data.MySqlClient;
using NtFreX.Blog.Configuration;
using NtFreX.Blog.Data.Models;

namespace NtFreX.Blog.Data.EfCore
{
    public class RelationalDbImageRepository : RelationalDbRepository<Models.ImageModel, ImageModel>, IImageRepository
    {
        private readonly MySqlConnectionFactory connectionFactory;
        private readonly IMapper mapper;
        private readonly ApplicationContextActivityDecorator applicationContextActivityDecorator;

        public RelationalDbImageRepository(MySqlConnectionFactory connectionFactory, IMapper mapper, ApplicationContextActivityDecorator applicationContextActivityDecorator)
            : base(connectionFactory, mapper, applicationContextActivityDecorator)
        {
            this.connectionFactory = connectionFactory;
            this.mapper = mapper;
            this.applicationContextActivityDecorator = applicationContextActivityDecorator;
        }

        public async Task<ImageModel> FindByNameAsync(string name)
        {
            var activity = applicationContextActivityDecorator.StartActivity($"{nameof(RelationalDbImageRepository)}.{nameof(FindByNameAsync)}");
            var dbModels = await connectionFactory.Connection.GetAllAsync<Models.ImageModel>();
            return mapper.Map<ImageModel>(dbModels.First(x => x.Name == name));
        }

        public static void EnsureTableExists(MySqlConnection connection)
        {
            connection.Execute(@"
create table if not exists `image` (
    `Id` varchar(255) not null unique, 
    `Name` text,
    `Type` varchar(255), 
    `Data` longtext, 
    primary key ( `Id` ) 
);");
        }
    }
}
