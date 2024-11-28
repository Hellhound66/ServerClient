using System.Net.Sockets;
using System.Text;
using Messages.Contracts;

namespace Messages.Extensions;

public static class TcpClientExtensions
{
    public static async Task SendMessage(this TcpClient client, IStreamableMessage message, CancellationToken cancellationToken)
    {
        message.MessageType = message.GetType().AssemblyQualifiedName!;
        await client.GetStream().WriteAsync(
            Encoding.Default.GetBytes(System.Text.Json.JsonSerializer.Serialize(message)), cancellationToken);
    }
}