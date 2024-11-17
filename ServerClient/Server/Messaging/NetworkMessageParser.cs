using System.Net.Sockets;
using System.Text.Json;
using Messages.Contracts;
using Messages.NetworkMessages;
using Server.Contracts;
using Server.Exceptions;

namespace Server.Messaging;

public class NetworkMessageParser(IMessageHub messageHub) : INetworkMessageParser
{
    #region INetworkMessageServer implementation
    
    public async Task ReactToIncomingData(TcpClient client, MemoryStream stream, int bytesRead, CancellationToken cancellationToken)
    {
        while (stream.Length > 0)
        {
            var bufferLength = (int)stream.Length;
            
            var localBuffer = new byte[bufferLength];
            stream.Seek(0, SeekOrigin.Begin);
            _ = await stream.ReadAsync(localBuffer.AsMemory(0, bufferLength), cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return;

            var bytesParsed = await ProcessMessageBuffer(localBuffer, bufferLength, cancellationToken);
            if (bytesParsed == 0)
                return;

            RemoveAlreadyParsedBytesFromStream(stream, bytesParsed);
        }
    }

    private static void RemoveAlreadyParsedBytesFromStream(MemoryStream stream, int bytesParsed)
    {
        var buf = stream.GetBuffer();            
        Buffer.BlockCopy(buf, bytesParsed, buf, 0, (int)stream.Length - bytesParsed);
        stream.SetLength(stream.Length - bytesParsed);
    }
    
    #endregion
    
    private async Task<int> ProcessMessageBuffer(byte[] localBuffer, int bufferLength, CancellationToken cancellationToken)
    {
        var (canBeParsed, size) = CanMessageBeParsed(localBuffer, bufferLength);
        if (!canBeParsed)
            return 0;
        
        var message = await DeserializeNetworkMessage(localBuffer, size, cancellationToken);
        messageHub.PushMessage(message!);
        
        return size;
    }

    private static (bool canBeParsed, int size) CanMessageBeParsed(byte[] localBuffer, int bufferLength)
    {
        // Check for deserialization.
        if (localBuffer[0] != '{')
            throw new InvalidNetworkMessageException("Invalid character at the beginning of a message.");

        var bracketDepth = 0;
        for (var counter = 0; counter < bufferLength; ++counter)
        {
            if (localBuffer[counter] == '{')
                bracketDepth++;
            if (localBuffer[counter] == '}')
                bracketDepth--;
            if (bracketDepth == 0) 
                return (true, ++counter);
        }

        return (false, default);
    }

    private static async Task<INetworkMessage?> DeserializeNetworkMessage(byte[] localBuffer, int size,
        CancellationToken cancellationToken)
    {
        var networkMessageBase = await JsonSerializer.DeserializeAsync<NetworkMessage>(new MemoryStream(
            localBuffer, 0, size), JsonSerializerOptions.Default, cancellationToken);
        
        if (networkMessageBase == null)
            throw new InvalidNetworkMessageException("Failed to deserialize message.");
        var messageType = Type.GetType(networkMessageBase.MessageType);
        if (messageType == null)
            throw new InvalidNetworkMessageException($"Invalid message type {networkMessageBase.MessageType}");
        
        return (INetworkMessage?)await JsonSerializer.DeserializeAsync(
            new MemoryStream(localBuffer, 0, size), 
            messageType,
            JsonSerializerOptions.Default, 
            cancellationToken);
    }
}