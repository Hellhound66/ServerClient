using Messages.Contracts;
using Server.Connections;

namespace Server.Messages;

internal class ClientConnectedMessage : IMessage
{
    public ConnectedClient? ConnectedClient { get; init; }
}