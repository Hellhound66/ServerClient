using Microsoft.Extensions.Hosting;
using Server.Contracts;

namespace Server.Messaging;

internal class MessageHubService(IMessageHub messageHub) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        messageHub.Start(cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}