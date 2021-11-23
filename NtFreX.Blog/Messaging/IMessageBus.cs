using System.Threading.Tasks;

namespace NtFreX.Blog.Messaging
{
    public interface IMessageBus
    {
        public Task SendMessageAsync(string bus, string message);
    }
}
