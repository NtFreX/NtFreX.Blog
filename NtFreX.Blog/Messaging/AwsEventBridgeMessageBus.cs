using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using NtFreX.Blog.Configuration;

namespace NtFreX.Blog.Messaging
{
    public class AwsEventBridgeMessageBus : IMessageBus
    {
        private readonly ConfigPreloader config;

        public AwsEventBridgeMessageBus(ConfigPreloader config)
        {
            this.config = config;
        }

        public async Task SendMessageAsync(string bus, string message)
        {
            var accessKeyId = config.Get(ConfigNames.AwsMessageBusAccessKeyId);
            var accessKey = config.Get(ConfigNames.AwsMessageBusAccessKey);
            var client = new AmazonEventBridgeClient(accessKeyId, accessKey);
            await client.PutEventsAsync(new PutEventsRequest
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
        }
    }
}
