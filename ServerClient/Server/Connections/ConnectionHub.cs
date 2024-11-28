using System.Collections.Concurrent;
using Messages.Contracts;
using Server.Contracts;
using Server.Messages;

namespace Server.Connections;

internal class ConnectionHub
{
    private readonly ConcurrentDictionary<Guid, ConnectedClient> _connections = [];

    public ConnectionHub(IMessageHub messageHub)
    {
        messageHub.RegisterListener<ClientConnectedMessage>(ReactToClientConnectedMessage);
    }

    public async Task SendMessage(IStreamableMessage message, Guid receiver, CancellationToken cancellationToken)
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
}