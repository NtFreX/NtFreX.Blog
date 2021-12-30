using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using NtFreX.Blog.Cache;
using NtFreX.Blog.Data;
using NtFreX.Blog.Data.Models;
using NtFreX.Blog.Models;

namespace NtFreX.Blog.Services
{
    public class ImageService
    {
        private readonly IImageRepository imageRepository;
        private readonly ApplicationCache cache;
        private readonly IMapper mapper;

        public ImageService(IImageRepository imageRepository, ApplicationCache cache, IMapper mapper)
        {
            this.imageRepository = imageRepository;
            this.cache = cache;
            this.mapper = mapper;
        }

        public async Task<IReadOnlyList<string>> GetAllNamesAsync()
        {
            var images = await GetAllAsync();
            return images.Select(x => x.Name).ToList();
        }

        public async Task<IReadOnlyList<ImageDto>> GetAllAsync()
        {
            var images = await cache.CacheAsync(
                CacheKeys.AllImages.Name,
                CacheKeys.AllImages.TimeToLive,
                () => imageRepository.FindAsync());

            return mapper.Map<IReadOnlyList<ImageDto>>(images);
        }

        public async Task<ImageDto> GetByNameAsync(string name)
        {
            var cacheKey = CacheKeys.Image;
            var image = await cache.CacheAsync(
                cacheKey.Name(name),
                cacheKey.TimeToLive,
                () => imageRepository.FindByNameAsync(name));

            return mapper.Map<ImageDto>(image);
        }

        public async Task AddAsync(string name, Stream data)
        {
            using var buffer = new MemoryStream();
            await data.CopyToAsync(buffer);

            var image = new ImageModel
            {
                Name = name,
                Data = Convert.ToBase64String(buffer.ToArray()),
                Type = $"image/{name.Substring(name.LastIndexOf(".") + 1)}"
            };

            await imageRepository.InsertOrUpdate(image);
            await cache.RemoveSaveAsync(CacheKeys.Image.Name(name));
            await cache.RemoveSaveAsync(CacheKeys.AllImages.Name);
        }

        public Stream ToStream(ImageDto image)
        {
            var bytes = Convert.FromBase64String(image.Data);
            return new MemoryStream(bytes);
        }
    }
}
