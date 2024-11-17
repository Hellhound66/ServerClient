using System.Net.Sockets;

namespace Server.Contracts;

public interface INetworkMessageParser
{
    Task ReactToIncomingData(TcpClient client, MemoryStream stream, int bytesRead, CancellationToken cancellationToken);
}