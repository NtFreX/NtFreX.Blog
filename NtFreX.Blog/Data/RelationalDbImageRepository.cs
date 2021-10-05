using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;
using MongoDB.Bson;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Data
{
    /*

    create table if not exists `image` (
       `Id` varchar(255) not null unique, 
       `Name` text,
       `Type` varchar(255), 
       `Data` longtext, 
       primary key ( `Id` ) 
    );

    */

    public class RelationalDbImageRepository : IImageRepository
    {
        private readonly MySqlDatabaseConnectionFactory connectionFactory;

        public RelationalDbImageRepository(MySqlDatabaseConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        public async Task<ImageModel> FindByName(string name)
            => (await connectionFactory.Connection.GetAllAsync<ImageModel>()).First(x => x.Name == name);

        public async Task InsertOrUpdate(ImageModel image)
        {
            if (!string.IsNullOrEmpty(image.Id) && await connectionFactory.Connection.GetAsync<ImageModel>(image.Id) != null)
                await connectionFactory.Connection.UpdateAsync(image);
            else
            {
                image.Id = Guid.NewGuid().ToString();
                await connectionFactory.Connection.InsertAsync(image);
            }
        }
    }
}
