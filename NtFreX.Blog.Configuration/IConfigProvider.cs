using System.Threading.Tasks;

namespace NtFreX.Blog.Configuration
{
    public interface IConfigProvider
    {
        public Task<string> GetAsync(string key);
    }
}
