using System.Threading.Tasks;
using Amazon.SQS;
using NtFreX.Blog.Configuration;
using NtFreX.ConfigFlow.DotNet;

namespace NtFreX.Blog.Messaging
{
    public class AwsSqsMessageBus : IMessageBus
    {
        private readonly ConfigPreloader config;

        public AwsSqsMessageBus(ConfigPreloader config)
        {
            this.config = config;
        }

        public async Task SendMessageAsync(string bus, string message)
        {
            var client = new AmazonSQSClient(config.Get(ConfigNames.AwsMessageBusAccessKeyId), config.Get(ConfigNames.AwsMessageBusAccessKey));
            await client.SendMessageAsync(bus, message);
        }
    }
}
