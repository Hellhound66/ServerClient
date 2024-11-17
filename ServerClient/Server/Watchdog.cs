using Messages.NetworkMessages;
using Microsoft.Extensions.Hosting;
using Server.Contracts;

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
    
    private static Task ReactToKeepAliveMessage(KeepAliveMessage message, CancellationToken token)
    {
        Console.WriteLine($"Received keep alive message. Sender {message.Identifier}, Time {message.SystemTime}");
        return Task.CompletedTask;
    }

    private Task ReactToShutDownServerMessage(ShutdownServerMessage message, CancellationToken token)
    {
        _hostedApplicationLifetime.StopApplication();
        
        return Task.CompletedTask;
    }

    #endregion
    
}