// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using Messages.Extensions;
using Messages.NetworkMessages;

var cts = new CancellationTokenSource();
var cancellationToken = cts.Token;

var foo = new TcpClient();
await foo.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8081), cancellationToken);


await foo.SendMessage(new ClientTriesToConnectMessage
{
    PublicName = "Foo",
    Identifier = Guid.NewGuid(),
    SystemTime = DateTime.Now,
}, cancellationToken);

await foo.SendMessage(new KeepAliveMessage
{
    Identifier = Guid.NewGuid(),
    SystemTime = DateTime.Now,
}, cancellationToken);

await foo.SendMessage(new ShutdownServerMessage
{
    Identifier = Guid.NewGuid(),
    SystemTime = DateTime.Now,
}, cancellationToken);

await foo.GetStream().FlushAsync();
await Task.Delay(500);
