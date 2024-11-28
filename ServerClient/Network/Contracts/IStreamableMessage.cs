namespace Messages.Contracts;

public interface IStreamableMessage : IMessage
{
    string MessageType { get; set; }
}