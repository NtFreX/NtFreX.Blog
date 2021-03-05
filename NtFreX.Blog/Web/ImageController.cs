using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;
using NtFreX.Blog.Auth;
using NtFreX.Blog.Cache;
using NtFreX.Blog.Data;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Web
{
    [ApiController, Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly AuthorizationManager authorizationManager;
        private readonly IDistributedCache cache;
        private IMongoCollection<ImageModel> collection;

        public ImageController(Database database, AuthorizationManager authorizationManager, IDistributedCache cache)
        {
            collection = database.Blog.GetCollection<ImageModel>("image");
            this.authorizationManager = authorizationManager;
            this.cache = cache;
        }

        [HttpGet, Route("{name}")]
        [ResponseCache(Duration = 60 * 60 * 24)]
        public async Task<ActionResult> GetAsync(string name)
        {
            var image = await cache.CacheAsync(CacheKeys.Image(name), CacheKeys.TimeToLive, async () =>
            {
                return await collection.Find(d => d.Name == name).FirstAsync();
            });

            var bytes = Convert.FromBase64String(image.Data);
            var stream = new MemoryStream(bytes);
            var result = new FileStreamResult(stream, image.Type);
            result.FileDownloadName = name;
            return result;
        }

        [HttpPost, Route("{name}")]
        public async Task<ActionResult> PostAsync(string name)
        {
            if (!authorizationManager.IsAdmin())
                return Unauthorized();

            using var buffer = new MemoryStream();
            await Request.Body.CopyToAsync(buffer);

            var image = new ImageModel
            {
                Name = name,
                Data = Convert.ToBase64String(buffer.ToArray()),
                Type = $"image/{name.Substring(name.LastIndexOf(".") + 1)}"
            };

            await collection.FindOneAndDeleteAsync(Builders<ImageModel>.Filter.Eq(x => x.Name, name));
            await collection.InsertOneAsync(image);
            await cache.RemoveAsync(CacheKeys.Image(name));
            return Ok();
        }
    }
}
