using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;
using Messages.Connections.Contracts;
using Messages.Contracts;
using Messages.Extensions;
using Messages.Messaging;
using Messages.Messaging.Contracts;

namespace Messages.Connections;

public sealed class ConnectedClient : IConnectedClient
{
    private const int MaximumBufferSize = 4096;
    
    private readonly IMessageHub _messageHub;
    private readonly TcpClient _client;
    private readonly Task _communicationTask;
    
    private readonly MemoryStream _receivingMemoryStream;
    private readonly MemoryStream _sendingMemoryStream;
    private readonly byte[] _receivingBuffer = new byte[MaximumBufferSize];
    private readonly byte[] _sendingBuffer = new byte[MaximumBufferSize];

    private readonly SemaphoreSlim _sendingSemaphore = new(1, 1);
    private bool _isRunning = true;
    

    public Guid Identifier { get; }
    
    public ConnectedClient(IMessageHub messageHub, TcpClient client, 
        CancellationToken cancellationToken)
    {
        _messageHub = messageHub;
        _client = client;
        Identifier = Guid.NewGuid();
        _receivingMemoryStream = new MemoryStream();
        _sendingMemoryStream = new MemoryStream();
        _communicationTask = CommunicationTask(cancellationToken);
    }

    public Task SendMessage(IStreamableMessage message, CancellationToken cancellationToken) => 
        _sendingSemaphore.Lock(ct => SerializeMessageToStream(message, ct), cancellationToken);

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
                    await Read(cancellationToken);
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

    private async Task Read(CancellationToken cancellationToken)
    {
        var networkStream = _client.GetStream();

        await ReadFromNetworkIntoMemoryStream(networkStream, cancellationToken);

        var localBuffer = await NetworkMessageParser.ReadMessageToLocalBuffer(_receivingMemoryStream, cancellationToken);
        var (canBeParsed, parsingSize) = NetworkMessageParser.CanMessageBeParsed(localBuffer, localBuffer.Length);
        if (!canBeParsed)
            return;
        var message = await NetworkMessageParser.DeserializeNetworkMessage(localBuffer, parsingSize, cancellationToken);
        
        // Todo: null reference check
        _messageHub.SendMessage(message!, Identifier);
        NetworkMessageParser.RemoveAlreadyParsedBytesFromStream(_receivingMemoryStream, parsingSize);
    }

    private async Task ReadFromNetworkIntoMemoryStream(NetworkStream networkStream, CancellationToken cancellationToken)
    {
        do
        {
            if (!networkStream.DataAvailable)
                break;
            
            var bytesRead = await networkStream.ReadAsync(_receivingBuffer, cancellationToken);
            if (bytesRead == 0)
                break;
        
            _receivingMemoryStream.Seek(0, SeekOrigin.End);
            await _receivingMemoryStream.WriteAsync(_receivingBuffer.AsMemory(0, bytesRead), cancellationToken);
            
        } while (!cancellationToken.IsCancellationRequested);
    }

    private void CheckHealthOfTcpConnection()
    {
        if (_client.GetState() is TcpState.Closed or TcpState.Closing or TcpState.CloseWait)
            _isRunning = false;
    }

    private async Task SerializeMessageToStream(IStreamableMessage message, CancellationToken cancellationToken)
    {
        _sendingMemoryStream.Seek(0, SeekOrigin.End);
        await JsonSerializer.SerializeAsync(_sendingMemoryStream, message, JsonSerializerOptions.Default,
            cancellationToken);
    }
}