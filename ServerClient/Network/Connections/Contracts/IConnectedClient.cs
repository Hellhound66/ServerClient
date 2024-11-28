using Messages.Contracts;

namespace Messages.Connections.Contracts;

public interface IConnectedClient
{
    Guid Identifier { get; }
    Task SendMessage(IStreamableMessage message, CancellationToken cancellationToken);
}