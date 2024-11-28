using System.Collections.Concurrent;
using Messages.Connections.Contracts;
using Messages.Contracts;
using Messages.Messages;
using Messages.Messaging.Contracts;

namespace Messages.Connections;

internal class ConnectionHub(IMessageHub messageHub) : IConnectionHub
{
    private readonly ConcurrentDictionary<Guid, IConnectedClient> _connections = [];

    public void Start()
    {
        messageHub.RegisterListener<ClientConnectedMessage>(ReactToClientConnectedMessage);
        messageHub.RegisterListener<SendNetworkMessage>(ReactToSendNetworkMessage);
    }

    private async Task SendMessage(IStreamableMessage message, Guid receiver, CancellationToken cancellationToken)
    {
        if (!_connections.TryGetValue(receiver, out var connectedClient))
            throw new InvalidOperationException($"Connected client {receiver} not found.");
        await connectedClient.SendMessage(message, cancellationToken);
    }

    private Task ReactToClientConnectedMessage(ClientConnectedMessage message, Guid sender, CancellationToken cancellationToken)
    {
        _connections[message.ConnectedClient!.Identifier] = message.ConnectedClient!;

        return Task.CompletedTask;
    }

    private Task ReactToSendNetworkMessage(SendNetworkMessage message, Guid sender, CancellationToken cancellationToken) => 
        SendMessage(message.MessageToSend, message.Receiver, cancellationToken);
}