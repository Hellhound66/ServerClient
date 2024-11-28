// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using Messages.Extensions;
using Messages.Messages;

namespace Client;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        var cts = new CancellationTokenSource();
        var ct = cts.Token;

        var clientTasks = Enumerable.Range(0, 10).Select(_ => CreateClient(ct)).ToArray();
        await Task.WhenAll(clientTasks);

        Console.ReadKey();
    }

    private static async Task CreateClient(CancellationToken cancellationToken)
    {
        var foo = new TcpClient();
        await foo.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8081), cancellationToken);
        foo.NoDelay = true;

        await foo.SendMessage(new KeepAliveMessage
        {
        }, cancellationToken);

        await foo.SendMessage(new KeepAliveMessage
        {
        }, cancellationToken);
        await foo.SendMessage(new KeepAliveMessage
        {
        }, cancellationToken);
        await foo.SendMessage(new KeepAliveMessage
        {
        }, cancellationToken);

        await Task.Delay(5000, cancellationToken);
    }

}