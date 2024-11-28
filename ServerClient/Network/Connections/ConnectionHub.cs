using System.Collections.Concurrent;
using Messages.Contracts;
using Messages.Messages;
using Messages.Messaging.Contracts;
using Microsoft.Extensions.Hosting;

namespace Messages.Connections;

internal class ConnectionHub(IMessageHub messageHub) : IHostedService
{
    private readonly ConcurrentDictionary<Guid, ConnectedClient> _connections = [];

    public Task StartAsync(CancellationToken cancellationToken)
    {
        messageHub.RegisterListener<ClientConnectedMessage>(ReactToClientConnectedMessage);
        messageHub.RegisterListener<SendNetworkMessage>(ReactToSendNetworkMessage);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
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