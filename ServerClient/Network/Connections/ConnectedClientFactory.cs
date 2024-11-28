using System.Net.Sockets;
using Messages.Connections.Contracts;
using Messages.Messaging.Contracts;

namespace Messages.Connections;

internal sealed class ConnectedClientFactory(IMessageHub messageHub)
    : IConnectedClientFactory
{
    public IConnectedClient Create(TcpClient client, CancellationToken cancellationToken) 
        => new ConnectedClient(messageHub, client, cancellationToken);
}