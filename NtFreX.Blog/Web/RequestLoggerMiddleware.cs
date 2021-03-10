using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using NtFreX.Blog.Data;
using NtFreX.Blog.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace NtFreX.Blog.Web
{
    public class RequestLoggerMiddleware
    {
        private readonly RequestDelegate next;

        public RequestLoggerMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context, Database database, IHostEnvironment hostEnvironment)
        {
            //var original = context.Response.Body;
            //using var tmp = new MemoryStream();
            //context.Response.Body = tmp;

            var requestBody = await ReadRequestBodyAsync(context);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await next.Invoke(context);
            stopwatch.Stop();

            var vulnerabilityCheck = VulnerabilityManager.Instance.CheckRequest(context.Request, context.Response, requestBody);

            var request = database.Monitoring.GetCollection<RequestModel>("request");
            await request.InsertOneAsync(new RequestModel
            {
                Date = DateTime.UtcNow,                
                Host = Environment.MachineName,
                System = "NtFrex.Blog",
                Environment = hostEnvironment.EnvironmentName,

                //Body = await ReadBodyAsync(context, tmp, original),
                StatusCode = context.Response.StatusCode,
                Success = vulnerabilityCheck.IsVulnerability 
                    ? !vulnerabilityCheck.HasFailed 
                    : IsSuccesStatusCode(context.Response.StatusCode.ToString()) || (Exceptions.TryGetValue(context.Request.Path, out var statusCode) && statusCode == context.Response.StatusCode),
                LatencyInMs = stopwatch.ElapsedMilliseconds,

                RemoteIp = context.Connection.RemoteIpAddress.ToString(),
                IsAttack = vulnerabilityCheck.IsVulnerability,

                Path = context.Request.Path,
                Method = context.Request.Method,
                Body = requestBody,
                RequestHeader = ToJson(context.Request.Headers),
                RequestHost = context.Request.Host.Value,
                RequestQueryString = context.Request.QueryString.ToString(),
                RequestScheme = context.Request.Scheme,
            });
        }

        private async Task<string> ReadRequestBodyAsync(HttpContext context)
        {
            context.Request.EnableBuffering();

            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var content = await reader.ReadToEndAsync();

            context.Request.Body.Seek(0, SeekOrigin.Begin);

            return content;
        }

        //private async Task<string> ReadBodyAsync(HttpContext context, Stream tmp, Stream original)
        //{
        //    tmp.Seek(0, SeekOrigin.Begin);
        //    var bodyText = await new StreamReader(context.Response.Body).ReadToEndAsync(); 
        //    tmp.Seek(0, SeekOrigin.Begin);
        //    await tmp.CopyToAsync(original);
        //    return bodyText;
        //}

        private string ToJson(IHeaderDictionary headers)
        {
            var jsonHeaders = new List<string>();
            foreach(var header in headers)
            {
                jsonHeaders.Add(header.Key.ToJson() + ":" + string.Join("; ", header.Value.ToArray()).ToJson());
            }
            return "{" + string.Join(", " + Environment.NewLine, jsonHeaders);
        }

        private bool IsSuccesStatusCode(string statusCode)
            => (statusCode.StartsWith("2") && statusCode.Length == 3)        // success
               || (statusCode.StartsWith("1") && statusCode.Length == 3)     // informational response
               || (statusCode.StartsWith("3") && statusCode.Length == 3);    // redirection

        private static Dictionary<string, int> Exceptions = new Dictionary<string, int>
        {
            { "/rss/", 404 },
            { "/rss.xml", 404 },
            { "/humans.txt", 404 },
            { "/ads.txt", 404 },
        };
    }
}
