using System.Text;
using System.Threading.Tasks;
using NtFreX.Blog.Configuration;
using RabbitMQ.Client;

namespace NtFreX.Blog.Messaging
{
    public class RabbitMessageBus : IMessageBus
    {
        private readonly ConfigPreloader config;

        public RabbitMessageBus(ConfigPreloader config)
        {
            this.config = config;
        }

        public Task SendMessageAsync(string bus, string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            var factory = new ConnectionFactory() { 
                HostName = config.Get(ConfigNames.RabbitMqHost), 
                UserName = config.Get(ConfigNames.RabbitMqUser), 
                Password = config.Get(ConfigNames.RabbitMqPassword) 
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: bus, durable: true, exclusive: false, autoDelete: false);
            channel.BasicPublish(exchange: string.Empty, routingKey: bus, body: body);
            return Task.CompletedTask;
        }
    }
}
