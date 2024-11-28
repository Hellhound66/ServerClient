using System.Text.Json;
using Messages.Contracts;
using Messages.Exceptions;
using Messages.Messages;

namespace Messages.Messaging;

public static class NetworkMessageParser
{
    #region Public static methods

    public static async Task<byte[]> ReadMessageToLocalBuffer(MemoryStream stream, 
        CancellationToken cancellationToken)
    {
        var bufferLength = (int)stream.Length;
            
        var localBuffer = new byte[bufferLength];
        stream.Seek(0, SeekOrigin.Begin);
        _ = await stream.ReadAsync(localBuffer.AsMemory(0, bufferLength), cancellationToken);
        return localBuffer;
    }

    public static (bool canBeParsed, int size) CanMessageBeParsed(byte[] localBuffer, int bufferLength)
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

    public static async Task<IStreamableMessage?> DeserializeNetworkMessage(byte[] localBuffer, int size,
        CancellationToken cancellationToken)
    {
        var networkMessageBase = await JsonSerializer.DeserializeAsync<StreamableMessage>(new MemoryStream(
            localBuffer, 0, size), JsonSerializerOptions.Default, cancellationToken);
        
        if (networkMessageBase == null)
            throw new InvalidNetworkMessageException("Failed to deserialize message.");
        var messageType = Type.GetType(networkMessageBase.MessageType);
        if (messageType == null)
            throw new InvalidNetworkMessageException($"Invalid message type {networkMessageBase.MessageType}");
        
        return (IStreamableMessage?)await JsonSerializer.DeserializeAsync(
            new MemoryStream(localBuffer, 0, size), 
            messageType,
            JsonSerializerOptions.Default, 
            cancellationToken);
    }

    public static IStreamableMessage PopulateNetworkMessage(IStreamableMessage streamableMessage)
    {
        streamableMessage.MessageType = streamableMessage.GetType().AssemblyQualifiedName!;
        return streamableMessage;
    }
    
    public static void RemoveAlreadyParsedBytesFromStream(MemoryStream stream, int bytesParsed)
    {
        var buf = stream.GetBuffer();            
        Buffer.BlockCopy(buf, bytesParsed, buf, 0, (int)stream.Length - bytesParsed);
        stream.SetLength(stream.Length - bytesParsed);
    }

    #endregion
    
    private static async Task<(int, IStreamableMessage)> ProcessMessageBuffer(byte[] localBuffer, int bufferLength, 
        CancellationToken cancellationToken)
    {
        var (canBeParsed, size) = CanMessageBeParsed(localBuffer, bufferLength);
        if (!canBeParsed)
            return (0, default)!;
        
        var message = await DeserializeNetworkMessage(localBuffer, size, cancellationToken);
        
        return (size, message)!;
    }
}