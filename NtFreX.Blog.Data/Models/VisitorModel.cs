using System;

namespace NtFreX.Blog.Models
{
    public class VisitorModel
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public string UserAgent { get; set; }
        public string Article { get; set; }
        public string RemoteIp { get; set; }
    }
}