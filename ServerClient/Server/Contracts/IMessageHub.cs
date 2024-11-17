using Messages.Contracts;

namespace Server.Contracts;

public interface IMessageHub
{
    void RegisterListener<T>(Func<T, CancellationToken, Task> action) where T : IMessage;
    void PushMessage(IMessage message);
    void Stop();
    Task Start(CancellationToken cancellationToken);
}