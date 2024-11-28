using System.Net.Sockets;

namespace Messages.Connections.Contracts;

public interface IConnectedClientFactory
{
    ConnectedClient Create(TcpClient client, CancellationToken cancellationToken);
}
