using System.Net.Sockets;
using Server.Contracts;

namespace Server.Connections;

internal sealed class ConnectedClientFactory(INetworkMessageParser networkMessageParser) : IConnectedClientFactory
{
    public ConnectedClient Create(TcpClient client, CancellationToken cancellationToken) 
        => new(networkMessageParser, client, cancellationToken);
}