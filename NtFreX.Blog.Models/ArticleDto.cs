using System;
using System.ComponentModel.DataAnnotations;

namespace NtFreX.Blog.Models
{
    public class ArticleDto
    {
        public string Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(500)]
        public string Subtitle { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public bool Published { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public bool IsPublished() => Published && Date <= DateTime.UtcNow;
    }
}
