// See https://aka.ms/new-console-template for more information

using Client;
using Messages;
using Messages.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Setup configuration.
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Setup options.
builder.Services.Configure<ServerOptions>(builder.Configuration.GetSection(nameof(ServerOptions)));

// Setup network
builder.AddNetwork();

// Setup DI
builder.Services
    .AddHostedService<ClientHostedService>();

// Start server as hosted service.
await builder.Build().StartAsync();