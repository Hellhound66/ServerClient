using Messages.Connections;
using Messages.Contracts;

namespace Messages.Messages;

public class ClientConnectedMessage : IMessage
{
    public ConnectedClient? ConnectedClient { get; init; }
}