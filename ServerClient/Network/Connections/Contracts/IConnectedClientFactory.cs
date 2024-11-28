using System.Net.Sockets;

namespace Messages.Connections.Contracts;

public interface IConnectedClientFactory
{
    IConnectedClient Create(TcpClient client, CancellationToken cancellationToken);
}
