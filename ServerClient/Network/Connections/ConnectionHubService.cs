using Messages.Connections.Contracts;
using Microsoft.Extensions.Hosting;

namespace Messages.Connections;

internal class ConnectionHubService(IConnectionHub connectionHub) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        connectionHub.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}