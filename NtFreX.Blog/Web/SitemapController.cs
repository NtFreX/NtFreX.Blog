using Microsoft.AspNetCore.Mvc;
using NtFreX.Blog.Services;
using System.Text;
using System.Threading.Tasks;

namespace NtFreX.Blog.Web
{
    public class SitemapController : ControllerBase
    {
        private readonly ArticleService articleService;

        public SitemapController(ArticleService articleService)
        {
            this.articleService = articleService;
        }

        [Route("/sitemap.xml")]
        public async Task GetSitemapAsync()
        {
            var host = Request.Scheme + "://" + Request.Host;

            Response.ContentType = "application/xml";
            await Response.Body.WriteAsync(Encoding.UTF8.GetBytes(@"<?xml version=""1.0"" encoding=""UTF-8""?>"));
            await Response.Body.WriteAsync(Encoding.UTF8.GetBytes(@"<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">"));
            

            await WriteUrlElementAsync(host);
            foreach(var article in await articleService.GetAllArticlesAsync(includeUnpublished: false))
            {
                await WriteUrlElementAsync($"{ host}/article/{article.Id}");
            }

            await Response.Body.WriteAsync(Encoding.UTF8.GetBytes(@"</urlset>"));
        }

        private async Task WriteUrlElementAsync(string path)
        {
            await Response.Body.WriteAsync(Encoding.UTF8.GetBytes(@"<url>"));
            await Response.Body.WriteAsync(Encoding.UTF8.GetBytes(@$"<loc>{path}</loc>"));
            await Response.Body.WriteAsync(Encoding.UTF8.GetBytes(@"</url>"));
        }
    }
}
