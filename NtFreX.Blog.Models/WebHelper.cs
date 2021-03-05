using System;
using System.Text;

namespace NtFreX.Blog.Models
{
    public static class WebHelper
    {
        public static string Base64UrlDecode(string value)
            => Encoding.UTF8.GetString(
                    Convert.FromBase64String(
                        value
                            .Replace("_", "/")
                            .Replace("-", "+")));
        
        public static string Base64UrlEncode(string value) 
            => Convert.ToBase64String(Encoding.UTF8.GetBytes(value))
                .Replace("+", "-")
                .Replace("/", "_");
    }
}
