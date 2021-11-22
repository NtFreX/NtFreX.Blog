using System;
using System.Text;

namespace NtFreX.Blog.Core
{
    public static class WebHelper
    {
        public static string Base64UrlDecode(string value, Encoding encoding = null)
            => (encoding ?? Encoding.UTF8).GetString(Base64UrlDecode(value));

        public static byte[] Base64UrlDecode(string value)
            => Convert.FromBase64String(
                        value
                            .Replace("_", "/")
                            .Replace("-", "+"));

        public static string Base64UrlEncode(string value, Encoding encoding = null)
            => Base64UrlEncode((encoding ?? Encoding.UTF8).GetBytes(value));

        public static string Base64UrlEncode(byte[] value)
            => Convert.ToBase64String(value)
                .Replace("+", "-")
                .Replace("/", "_");
    }
}
