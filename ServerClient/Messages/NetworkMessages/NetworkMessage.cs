using Messages.Contracts;

namespace Messages.NetworkMessages;

public class NetworkMessage : INetworkMessage
{
    public string MessageType { get; set; } = "Unknown";
    public required Guid Identifier { get; init; }
    public required DateTime SystemTime { get; init; }

   // protected NetworkMessage() => MessageType = GetType().AssemblyQualifiedName!;
}