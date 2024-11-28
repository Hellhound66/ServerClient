using Messages.Messages;
using Messages.Messaging.Contracts;
using Microsoft.Extensions.Hosting;

namespace Server;

public class Watchdog
{
    private readonly IHostApplicationLifetime _hostedApplicationLifetime;

    public Watchdog(IMessageHub messageHub, IHostApplicationLifetime hostedApplicationLifetime)
    {
        _hostedApplicationLifetime = hostedApplicationLifetime;
        
        messageHub.RegisterListener<KeepAliveMessage>(ReactToKeepAliveMessage);
        messageHub.RegisterListener<ShutdownServerMessage>(ReactToShutDownServerMessage);
    }
    
    #region Messages
    
    private static Task ReactToKeepAliveMessage(KeepAliveMessage keepAliveMessage, Guid sender, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Received keep alive message. Sender {sender}, time {DateTime.Now}");
        return Task.CompletedTask;
    }

    private Task ReactToShutDownServerMessage(ShutdownServerMessage shutdownServerMessage, Guid sender, CancellationToken cancellationToken)
    {
        _hostedApplicationLifetime.StopApplication();
        
        return Task.CompletedTask;
    }

    #endregion
    
}