using System.Linq;
using System.Security.Cryptography;

namespace NtFreX.Blog.Core
{
    public class RandomExtensions
    {
        public static string GetRandomNumberString(int length)
            => new string(Enumerable.Repeat(0, length).Select(x => RandomNumberGenerator.GetInt32(0, 10).ToString()[0]).ToArray());
    }
}
