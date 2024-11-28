using Messages.Contracts;

namespace Messages.NetworkMessages;

public class StreamableMessage : IStreamableMessage
{
    public string MessageType { get; set; } = "Unknown";
}