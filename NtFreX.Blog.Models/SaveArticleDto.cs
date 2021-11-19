using System.ComponentModel.DataAnnotations;

namespace NtFreX.Blog.Models
{
    public class SaveArticleDto
    {
        [Required]
        public ArticleDto Article { get; set; }
        [Required]
        public string[] Tags { get; set; }
    }
}
