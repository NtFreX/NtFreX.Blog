namespace NtFreX.Blog.Models
{
    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public LoginResponseType Type { get; set; }
        public string Value { get; set; }

        public static LoginResponseDto Failed() => new LoginResponseDto { Success = false, Type = LoginResponseType.None };
    }
}
