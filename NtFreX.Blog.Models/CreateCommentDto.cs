using System.ComponentModel.DataAnnotations;

namespace NtFreX.Blog.Models
{
    public class CreateCommentDto
    {
        [Required]
        public string ArticleId { get; set; }
        public string User { get; set; }
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        public string CaptchaResponse { get; set; }
    }
}
