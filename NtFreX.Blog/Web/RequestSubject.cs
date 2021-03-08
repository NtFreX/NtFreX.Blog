using Microsoft.AspNetCore.Http;
using System;
using System.Net;

namespace NtFreX.Blog.Web
{
    public class RequestSubject
    {
        public string RequestScheme { get; set; }
        public string RequestMethod { get; set; }
        public string RequestPath { get; set; }
        public string RequestBody { get; set; }

        public Func<HttpResponse, bool> HasFailed { get; set; }

        public static RequestSubject Create(string method, string path, string requestScheme, HttpStatusCode failureStatusCode)
            => Create(method, path, requestScheme, response => response.StatusCode == (int)failureStatusCode);

        public static RequestSubject Create(string method, string path, HttpStatusCode failureStatusCode)
            => Create(method, path, null, response => response.StatusCode == (int) failureStatusCode);

        public static RequestSubject Create(string method, string path, string requestScheme, Func<HttpResponse, bool> hasFailed)
            => new RequestSubject { RequestMethod = method, RequestPath = path, RequestScheme = requestScheme, HasFailed = hasFailed };
    }
}
