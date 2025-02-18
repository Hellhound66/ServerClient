using System.Net;
using System.Net.Sockets;
using Messages;
using Messages.Connections.Contracts;
using Messages.Messages;
using Messages.Messaging.Contracts;
using Microsoft.Extensions.Hosting;

namespace Client;

public class ClientHostedService(IConnectedClientFactory connectedClientFactory, 
    IMessageHub messageHub,
    ServerOptions options) : IHostedService
{
    private static readonly IPAddress IpString = IPAddress.Parse("127.0.0.1");
    private static readonly Guid ClientGuid = Guid.NewGuid();

    private Guid _serverGuid;
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Connect(cancellationToken);

        messageHub.SendMessage(new SendNetworkMessage
        {
            Receiver = _serverGuid,
            MessageToSend = new KeepAliveMessage
            {
            }
        }, _serverGuid);

        await Task.Delay(5000, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task Connect(CancellationToken cancellationToken)
    {
        var foo = new TcpClient();
        await foo.ConnectAsync(new IPEndPoint(IpString, options.Port), cancellationToken);
        foo.NoDelay = true;
        var connectedClient = connectedClientFactory.Create(foo, cancellationToken);
        messageHub.SendMessage(new ClientConnectedMessage
        {
            ConnectedClient = connectedClient,
        }, ClientGuid);

        _serverGuid = connectedClient.Identifier;
    }

}