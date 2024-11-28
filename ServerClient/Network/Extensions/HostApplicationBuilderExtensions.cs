using Messages.Connections;
using Messages.Connections.Contracts;
using Messages.Messaging;
using Messages.Messaging.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Messages.Extensions;

public static class HostApplicationBuilderExtensions
{
    public static HostApplicationBuilder AddNetwork(this HostApplicationBuilder builder)
    {
        builder.Services
            .AddHostedService<MessageHubHostedService>()
            .AddHostedService<ConnectionHubService>()
            .AddSingleton<IMessageHub, MessageHub>()
            .AddSingleton<IConnectionHub, ConnectionHub>()
            .AddSingleton<IConnectedClientFactory, ConnectedClientFactory>();
        return builder;
    }
}