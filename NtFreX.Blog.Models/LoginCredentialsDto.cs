namespace NtFreX.Blog.Models
{
    public class LoginCredentialsDto
    {
        public string Key { get; set; }
        public string Secret { get; set; }
        public string Session { get; set; }
        public LoginCredentialsType Type { get; set; }
        public string CaptchaResponse { get; set; }
    }
}
