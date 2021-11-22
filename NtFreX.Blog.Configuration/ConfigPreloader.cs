using System.Collections.Generic;
using System.Threading.Tasks;

namespace NtFreX.Blog.Configuration
{
    public class ConfigPreloader
    {
        private Dictionary<string, string> configs = new Dictionary<string, string>();
        private readonly IConfigProvider provider;

        private ConfigPreloader(IConfigProvider provider)
        {
            this.provider = provider;
        }

        public string Get(string key)
            => configs[key];

        public bool TryGet(string key, out string value)
        {
            var result = configs.TryGetValue(key, out var cached);
            value = cached;
            return result;
        }

        public async Task LoadByKeysAsync(params string[] keys)
        {
            foreach (var key in keys)
            {
                configs.Add(key, await provider.GetAsync(key));
            }
        }

        public static async Task<ConfigPreloader> LoadAsync(IConfigProvider provider, params string[] keys)
        {
            var loader = new ConfigPreloader(provider);
            await loader.LoadByKeysAsync(keys);
            return loader;
        }
    }
}
