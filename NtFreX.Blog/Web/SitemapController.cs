using Microsoft.AspNetCore.Mvc;
using NtFreX.Blog.Services;
using System.Threading.Tasks;
using System.Xml;

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
            using (var xml = XmlWriter.Create(Response.Body, new XmlWriterSettings { Indent = true, Async = true }))
            {
                xml.WriteStartDocument();
                xml.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

                WriteUrlElement(xml, host);
                foreach(var article in await articleService.GetAllArticlesAsync(false))
                {
                    WriteUrlElement(xml, $"{host}/article/{article.Id}");
                }

                xml.WriteEndElement();

                await xml.FlushAsync();
            }
        }

        private void WriteUrlElement(XmlWriter xml, string path)
        {
            xml.WriteStartElement("url");
            xml.WriteElementString("loc", path);
            xml.WriteEndElement();
        }
    }
}
