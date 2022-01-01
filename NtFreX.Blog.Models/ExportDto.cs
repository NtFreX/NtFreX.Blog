using System.Collections.Generic;

namespace NtFreX.Blog.Models
{
    public class ExportDto
    {
        public TagDto[] Tags { get; set; }
        public ArticleDto[] Articles { get; set; }
        public VisitorDto[] Visitors { get; set; }
        public CommentDto[] Comments { get; set; }
        public ImageDto[] Images { get; set; }
    }
}
