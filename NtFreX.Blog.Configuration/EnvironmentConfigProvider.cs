using System.Threading.Tasks;

namespace NtFreX.Blog.Configuration
{
    public class EnvironmentConfigProvider : IConfigProvider
    {
        public Task<string> GetAsync(string key)
            => Task.FromResult(System.Environment.GetEnvironmentVariable(key));
    }
}
