using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Server.Connections;
using Server.Contracts;
using Server.Messages;

namespace Server;

internal class ServerHostedService(
    IOptions<ServerOptions> serverOptions,
    IMessageHub messageHub,
    ConnectedClientFactory connectedClientFactory,
    Watchdog watchdog,
    ConnectionHub connectionHub) : IHostedService
{
    private readonly Watchdog _watchdog = watchdog;
    private readonly ConnectionHub _connectionHub = connectionHub;

    public static Guid ServerIdentifier { get; } = Guid.NewGuid();

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

    public Task StopAsync(CancellationToken cancellationToken)
    {
        messageHub.Stop();
        return Task.CompletedTask;
    }

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