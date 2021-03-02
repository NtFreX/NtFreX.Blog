using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace NtFreX.Blog.Model
{
    public class RequestModel
    {
        [BsonElement("id", Order = 0)]
        public ObjectId Id { get; set; }

        [BsonElement("date", Order = 1)]
        public DateTime Date { get; set; }

        [BsonElement("status_code", Order = 2)]
        public int StatusCode { get; set; }

        [BsonElement("remote_ip", Order = 3)]
        public string RemoteIp { get; set; }

        [BsonElement("success", Order = 4)]
        public bool Success { get; set; }

        [BsonElement("request_header", Order = 5)]
        public string RequestHeader { get; set; }

        [BsonElement("latency_in_ms", Order = 6)]
        public long LatencyInMs { get; set; }

        [BsonElement("system", Order = 7)]
        public string System { get; set; }

        [BsonElement("host", Order = 8)]
        public string Host { get; set; }

        [BsonElement("path", Order = 9)]
        public string Path { get; set; }

        [BsonElement("is_attack", Order = 10)]
        public bool IsAttack { get; set; }

        [BsonElement("method", Order = 11)]
        public string Method { get; set; }

        [BsonElement("body", Order = 12)]
        public string Body { get; set; }

        [BsonElement("request_scheme", Order = 13)]
        public string RequestScheme { get; set; }

        [BsonElement("request_host", Order = 14)]
        public string RequestHost { get; set; }
    }
}
