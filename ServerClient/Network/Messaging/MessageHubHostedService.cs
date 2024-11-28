using Messages.Messaging.Contracts;
using Microsoft.Extensions.Hosting;

namespace Messages.Messaging;

public class MessageHubHostedService(IMessageHub messageHub) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        messageHub.Start(cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}