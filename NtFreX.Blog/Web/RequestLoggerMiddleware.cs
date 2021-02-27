using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using NtFreX.Blog.Data;
using NtFreX.Blog.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public async Task InvokeAsync(HttpContext context, Database database)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await next.Invoke(context);
            stopwatch.Stop();

            var request = database.Monitoring.GetCollection<RequestModel>("request");
            await request.InsertOneAsync(new RequestModel
            {
                Date = DateTime.UtcNow,
                RemoteIp = context.Connection.RemoteIpAddress.ToString(),
                RequestHeader = ToJson(context.Request.Headers),
                StatusCode = context.Response.StatusCode,
                Success = IsSuccesStatusCode(context.Response.StatusCode.ToString()),
                LatencyInMs = stopwatch.ElapsedMilliseconds,
                Host = Environment.MachineName,
                System = "NtFrex.Blog",
                Path = context.Request.Path
            });
        }

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
    }
}
