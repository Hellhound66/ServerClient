namespace Messages.Contracts;

public interface INetworkMessage : IMessage
{
    string MessageType { get; set; }
    Guid Identifier { get; }
    DateTime SystemTime { get; }
}