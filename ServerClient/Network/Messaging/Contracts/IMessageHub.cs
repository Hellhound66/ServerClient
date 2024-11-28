using Messages.Contracts;

namespace Messages.Messaging.Contracts;

public interface IMessageHub
{
    void RegisterListener<T>(Func<T, Guid, CancellationToken, Task> action) where T : IMessage;

    void SendMessage(IMessage message, Guid sender);
    void Stop();
    Task Start(CancellationToken cancellationToken);
}