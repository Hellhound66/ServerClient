namespace Messages.NetworkMessages;

public class ClientTriesToConnectMessage : NetworkMessage
{
    public string PublicName { get; init; } = string.Empty;
}