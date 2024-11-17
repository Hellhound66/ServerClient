﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Server;
using Server.Connections;
using Server.Contracts;
using Server.Messaging;

var builder = Host.CreateApplicationBuilder(args);

// Setup configuration.
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Setup options.
builder.Services.Configure<ServerOptions>(builder.Configuration.GetSection(nameof(ServerOptions)));

// Setup DI
builder.Services
    .AddHostedService<MessageHubService>()
    .AddHostedService<ServerHostedService>()
    .AddSingleton<INetworkMessageParser, NetworkMessageParser>()
    .AddSingleton<IMessageHub, MessageHub>()
    .AddSingleton<ConnectedClientFactory>()
    .AddSingleton<Watchdog>();

// Start server as hosted service.
await builder.Build().StartAsync();