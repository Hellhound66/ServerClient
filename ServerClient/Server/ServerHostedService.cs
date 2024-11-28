using System.Net;
using System.Net.Sockets;
using Messages.Connections.Contracts;
using Messages.Messages;
using Messages.Messaging.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Server;

internal class ServerHostedService(
    IOptions<ServerOptions> serverOptions,
    IMessageHub messageHub,
    IConnectedClientFactory connectedClientFactory,
    Watchdog watchdog) : IHostedService
{
    private readonly Watchdog _watchdog = watchdog;

    private static Guid ServerIdentifier { get; } = Guid.NewGuid();

    #region IHostedService implementation
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await StartListeningToNetwork(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        finally
        {
            await StopAsync(CancellationToken.None);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    #endregion

    private async Task StartListeningToNetwork(CancellationToken cancellationToken)
    {
        using var listener = new TcpListener(new IPEndPoint(IPAddress.Any, serverOptions.Value.Port)); 
        listener.Start();

        while (!cancellationToken.IsCancellationRequested)
        {
            var client = await listener.AcceptTcpClientAsync(cancellationToken);

            var connectedClient = connectedClientFactory.Create(client, cancellationToken);

            messageHub.SendMessage(new ClientConnectedMessage
            {
                ConnectedClient = connectedClient,
            }, ServerIdentifier);
        }
    }
}