using Messages.Contracts;

namespace Messages.Messages;

public class SendNetworkMessage : IMessage
{
    public required IStreamableMessage MessageToSend { get; set; }
    public required Guid Receiver { get; set; }
}