using System.Net.Sockets;
using Server.Connections;

namespace Server.Contracts;

internal interface IConnectedClientFactory
{
    ConnectedClient Create(TcpClient client, CancellationToken cancellationToken);
}
