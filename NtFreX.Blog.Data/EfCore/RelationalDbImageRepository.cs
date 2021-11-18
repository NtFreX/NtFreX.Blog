using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Dapper.Contrib.Extensions;
using MySql.Data.MySqlClient;
using NtFreX.Blog.Configuration;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Data.EfCore
{
    public class RelationalDbImageRepository : RelationalDbRepository<Models.ImageModel, ImageModel>, IImageRepository
    {
        private readonly MySqlDatabaseConnectionFactory connectionFactory;
        private readonly IMapper mapper;

        public RelationalDbImageRepository(MySqlDatabaseConnectionFactory connectionFactory, IMapper mapper)
            : base(connectionFactory, mapper)
        {
            this.connectionFactory = connectionFactory;
            this.mapper = mapper;
        }

        public async Task<ImageModel> FindByName(string name)
        {
            var activitySource = new ActivitySource(BlogConfiguration.ActivitySourceName);
            using (var sampleActivity = activitySource.StartActivity($"{nameof(RelationalDbImageRepository)}.{nameof(FindByName)}", ActivityKind.Server))
            {
                var dbModels = await connectionFactory.Connection.GetAllAsync<Models.ImageModel>();
                return mapper.Map<ImageModel>(dbModels.First(x => x.Name == name));
            }
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
