using Messages.Contracts;

namespace Messages.Messages;

public class StreamableMessage : IStreamableMessage
{
    public string MessageType { get; set; } = "Unknown";
}