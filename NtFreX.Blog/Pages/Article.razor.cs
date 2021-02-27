using Microsoft.AspNetCore.Components;
using MongoDB.Bson;
using NtFreX.Blog.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NtFreX.Blog.Pages
{
    public partial class Article : ComponentBase
    {
        private ArticleModel article;
        private IReadOnlyList<ArticleModel> topArticles;
        private IReadOnlyList<CommentModel> comments;
        private IReadOnlyList<TagModel> tags;
        private CommentModel newComment;

        [Parameter] public string Id { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            newComment = new CommentModel();
            newComment.ArticleId = new ObjectId(Id);
            topArticles = await articleService.GetTopTreeArticlesAsync(Id);
            article = await articleService.GetArticleByIdAsync(Id);
            comments = await commentService.GetCommentsByArticleIdAsync(Id);
            tags = await tagService.GetTagsByArticleIdAsync(Id);

            await articleService.VisitArticleAsync(Id);
        }

        private async Task InsertCommentAsync()
        {
            if (string.IsNullOrEmpty(newComment.Content) && string.IsNullOrEmpty(newComment.Title))
                return;

            await commentService.InsertCommentAsync(newComment);

            await OnParametersSetAsync();
        }
    }
}
