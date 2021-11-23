using NtFreX.Blog.Core;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NtFreX.Blog.Configuration
{
    public class WebConfigProvider : IConfigProvider
    {
        private readonly HttpClient httpClient;

        public WebConfigProvider(string clientId, string clientSecret, string path)
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(path);
            httpClient.DefaultRequestHeaders.Add("X-ClientId", clientId);
            httpClient.DefaultRequestHeaders.Add("X-ClientSecret", clientSecret);
        }

        public async Task<string> GetAsync(string key)
            => await httpClient.GetStringAsync($"/config/{WebHelper.Base64UrlEncode(key)}");
    }
}
