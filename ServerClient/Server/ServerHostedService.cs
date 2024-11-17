using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Server.Connections;
using Server.Contracts;

namespace Server;

internal class ServerHostedService(
    IOptions<ServerOptions> serverOptions,
    IMessageHub messageHub,
    ConnectedClientFactory connectedClientFactory,
    Watchdog _) : IHostedService
{
    private readonly List<ConnectedClient> _connections = [];
    private Task _networkListenerTask = Task.CompletedTask;
    private SemaphoreSlim _endApplicationSemaphore = new(0);
    
    #region IHostedService implementation
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _networkListenerTask = StartListeningToNetwork(cancellationToken);

        try
        {
            await _endApplicationSemaphore.WaitAsync(cancellationToken);        
        }
        catch (OperationCanceledException)
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
            
            _connections.Add(connectedClientFactory.Create(client, cancellationToken));
        }
    }
}