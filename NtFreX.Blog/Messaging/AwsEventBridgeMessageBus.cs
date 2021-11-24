using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using Microsoft.Extensions.Logging;
using NtFreX.Blog.Configuration;

namespace NtFreX.Blog.Messaging
{
    public class AwsEventBridgeMessageBus : IMessageBus
    {
        private readonly ConfigPreloader config;
        private readonly ILogger<AwsEventBridgeMessageBus> logger;

        public AwsEventBridgeMessageBus(ConfigPreloader config, ILogger<AwsEventBridgeMessageBus> logger)
        {
            this.config = config;
            this.logger = logger;
        }

        public async Task SendMessageAsync(string bus, string message)
        {
            var accessKeyId = config.Get(ConfigNames.AwsMessageBusAccessKeyId);
            var accessKey = config.Get(ConfigNames.AwsMessageBusAccessKey);
            var client = new AmazonEventBridgeClient(accessKeyId, accessKey);
            var response = await client.PutEventsAsync(new PutEventsRequest
            {
                Entries = new List<PutEventsRequestEntry>
                {
                    new PutEventsRequestEntry
                    {
                        EventBusName = bus,
                        DetailType = "ntfrex.blog.model",
                        Source = "ntfrex.blog.web",
                        Detail = message
                    }
                }
            });
            logger.LogDebug($"called AwsEventBridgeMessageBus service with {bus} and {message}");
            logger.LogDebug(JsonSerializer.Serialize(response));
        }
    }
}
