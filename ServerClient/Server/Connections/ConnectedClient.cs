using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;
using Messages.Contracts;
using Server.Contracts;
using Server.Extensions;

namespace Server.Connections;

internal sealed class ConnectedClient
{
    private const int MaximumBufferSize = 4096;
    
    private readonly INetworkMessageParser _networkMessageParser;
    private readonly TcpClient _client;
    private readonly Task _communicationTask;
    
    private readonly MemoryStream _receivingMemoryStream;
    private readonly MemoryStream _sendingMemoryStream;
    private readonly byte[] _receivingBuffer = new byte[MaximumBufferSize];
    private readonly byte[] _sendingBuffer = new byte[MaximumBufferSize];

    private readonly SemaphoreSlim _sendingSemaphore = new(1, 1);
    private bool _isRunning = true;
    

    public Guid Identifier { get; }
    
    public ConnectedClient(INetworkMessageParser networkMessageParser, TcpClient client, 
        CancellationToken cancellationToken)
    {
        _networkMessageParser = networkMessageParser;
        _client = client;
        Identifier = Guid.NewGuid();
        _receivingMemoryStream = new MemoryStream();
        _sendingMemoryStream = new MemoryStream();
        _communicationTask = CommunicationTask(cancellationToken);
    }

    private void CloseConnection()
    {
        _isRunning = false;
        _client.Close();
        _communicationTask.Wait();
    }

    private async Task CommunicationTask(CancellationToken cancellationToken)
    {
        while (_isRunning)
        {
            try
            {
                CheckHealthOfTcpConnection();
                if (!_isRunning)
                    break;
                
                await WriteToNetworkStream(cancellationToken);

                if (IsDataAvailable())
                    await ReadFromNetworkStream(cancellationToken);
                else 
                    await Yield(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Connected client canceled an operation.");
                _isRunning = false;
            }
        }
        
        CloseConnection();
    }

    private async Task WriteToNetworkStream(CancellationToken cancellationToken)
    {
        var bytesRead = await _sendingSemaphore.Lock(CropFromSendingStream, cancellationToken);
        await _client.Client.SendAsync(new ArraySegment<byte>(_sendingBuffer, 0, bytesRead), cancellationToken);
    }

    private async Task<int> CropFromSendingStream(CancellationToken cancellationToken)
    {
        if (_sendingMemoryStream.Length == 0)
            return 0;

        _sendingMemoryStream.Seek(0, SeekOrigin.Begin);
        return await _sendingMemoryStream.CropAsync(_sendingBuffer, MaximumBufferSize, cancellationToken);
    }

    private static Task Yield(CancellationToken cancellationToken) => Task.Delay(1, cancellationToken);

    private  bool IsDataAvailable() => _client.GetStream().DataAvailable;

    private async Task ReadFromNetworkStream(CancellationToken cancellationToken)
    {
        var bytesRead = 0;
        var networkStream = _client.GetStream();

        do
        {
            if (!networkStream.DataAvailable)
                break;
            
            bytesRead = await networkStream.ReadAsync(_receivingBuffer, cancellationToken);
            if (bytesRead == 0)
                break;
        
            _receivingMemoryStream.Seek(0, SeekOrigin.End);
            await _receivingMemoryStream.WriteAsync(_receivingBuffer.AsMemory(0, bytesRead), cancellationToken);
            
        } while (!cancellationToken.IsCancellationRequested);
        
        await _networkMessageParser.ReactToIncomingData(Identifier, _receivingMemoryStream, bytesRead, cancellationToken);
    }

    private void CheckHealthOfTcpConnection()
    {
        if (_client.GetState() is TcpState.Closed or TcpState.Closing or TcpState.CloseWait)
            _isRunning = false;
    }

    public Task SendMessage(IStreamableMessage message, CancellationToken cancellationToken) => 
        _sendingSemaphore.Lock(ct => SerializeMessageToStream(message, ct), cancellationToken);

    private async Task SerializeMessageToStream(IStreamableMessage message, CancellationToken cancellationToken)
    {
        _sendingMemoryStream.Seek(0, SeekOrigin.End);
        await JsonSerializer.SerializeAsync(_sendingMemoryStream, message, JsonSerializerOptions.Default,
            cancellationToken);
    }
}