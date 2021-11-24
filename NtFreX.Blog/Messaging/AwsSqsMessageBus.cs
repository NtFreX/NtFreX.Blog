using System.Text.Json;
using System.Threading.Tasks;
using Amazon.SQS;
using Microsoft.Extensions.Logging;
using NtFreX.Blog.Configuration;

namespace NtFreX.Blog.Messaging
{
    public class AwsSqsMessageBus : IMessageBus
    {
        private readonly ConfigPreloader config;
        private readonly ILogger<AwsSqsMessageBus> logger;

        public AwsSqsMessageBus(ConfigPreloader config, ILogger<AwsSqsMessageBus> logger)
        {
            this.config = config;
            this.logger = logger;
        }

        public async Task SendMessageAsync(string bus, string message)
        {
            var client = new AmazonSQSClient(config.Get(ConfigNames.AwsMessageBusAccessKeyId), config.Get(ConfigNames.AwsMessageBusAccessKey));
            var response = await client.SendMessageAsync(bus, message);

            logger.LogDebug($"called AwsSqsMessageBus service with {bus} and {message}");
            logger.LogDebug(JsonSerializer.Serialize(response));
        }
    }
}
