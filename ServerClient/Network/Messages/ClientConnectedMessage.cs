using Messages.Connections;
using Messages.Connections.Contracts;
using Messages.Contracts;

namespace Messages.Messages;

public class ClientConnectedMessage : IMessage
{
    public IConnectedClient? ConnectedClient { get; init; }
}