using System.Net.Sockets;
using Messages.Contracts;

namespace Server.Contracts;

public interface INetworkMessageParser
{
    Task ReactToIncomingData(Guid sender, MemoryStream stream, int bytesRead,
        CancellationToken cancellationToken);
}