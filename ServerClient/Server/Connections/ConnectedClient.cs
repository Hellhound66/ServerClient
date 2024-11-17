using System.Net.NetworkInformation;
using System.Net.Sockets;
using Server.Contracts;
using Server.Extensions;

namespace Server.Connections;

internal sealed class ConnectedClient
{
    private readonly INetworkMessageParser _networkMessageParser;
    private readonly TcpClient _client;
    private readonly Task _readingTask;
    private readonly Task _sendingTask;
    private readonly MemoryStream _memoryStream;
    
    private bool _isRunning = true;
    private readonly byte[] _receivingBuffer = new byte[65536]; 
    
    public ConnectedClient(INetworkMessageParser networkMessageParser,
        TcpClient client, CancellationToken cancellationToken)
    {
        _networkMessageParser = networkMessageParser;
        _client = client;
        _memoryStream = new MemoryStream();
        _readingTask = Reading(cancellationToken);
        _sendingTask = Writing(cancellationToken);
    }

    private void CloseConnection()
    {
        _isRunning = false;
        _client.Close();
    }

    private async Task Writing(CancellationToken cancellationToken)
    {
        while(_isRunning)
            await Task.Delay(10, cancellationToken);
    }

    private async Task Reading(CancellationToken cancellationToken)
    {
        while (_isRunning)
        {
            try
            {
                CheckHealthOfTcpConnection();

                if (!IsDataAvailable())
                {
                    await Yield(cancellationToken);
                    continue;
                }

                await ReadFromNetworkStream(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Connected client canceled an operation.");
                CloseConnection();
            }
        }
    }

    private static Task Yield(CancellationToken cancellationToken) => Task.Delay(1, cancellationToken);

    private  bool IsDataAvailable() => _client.GetStream().DataAvailable;

    private async Task ReadFromNetworkStream(CancellationToken cancellationToken)
    {
        var bytesRead = await _client.GetStream().ReadAsync(_receivingBuffer, cancellationToken);
        if (bytesRead == 0)
            return;
        
        _memoryStream.Seek(0, SeekOrigin.End);
        await _memoryStream.WriteAsync(_receivingBuffer.AsMemory(0, bytesRead), cancellationToken);
        
        await _networkMessageParser.ReactToIncomingData(_client, _memoryStream, bytesRead, cancellationToken);
    }

    private void CheckHealthOfTcpConnection()
    {
        if (_client.GetState() is TcpState.Closed or TcpState.Closing or TcpState.CloseWait)
            CloseConnection();
    }
}