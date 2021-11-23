using System.Threading.Tasks;

namespace NtFreX.Blog.Messaging
{
    public class NullMessageBus : IMessageBus
    {
        public Task SendMessageAsync(string bus, string message)
            => Task.CompletedTask;
    }
}
