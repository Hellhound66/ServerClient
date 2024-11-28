using System.Collections.Concurrent;
using Messages.Contracts;
using Messages.Exceptions;
using Messages.Messaging.Contracts;

namespace Messages.Messaging;

public class MessageHub : IMessageHub
{
    private readonly ConcurrentDictionary<Type, List<Func<IMessage, Guid, CancellationToken, Task>>> _messageListener = new();
    private readonly ConcurrentQueue<(IMessage, Guid)> _messageQueue = new();
    private readonly SemaphoreSlim _queueSemaphore = new(0);

    private bool _isRunning = true;

    public void RegisterListener<T>(Func<T, Guid, CancellationToken, Task> action) where T : IMessage
    {
        if (!_messageListener.TryGetValue(typeof(T), out var listenerList))
        {
            listenerList = [];
            _messageListener.TryAdd(typeof(T), listenerList);
        }

        listenerList.Add((message, sender, token) => action((T)message, sender, token));
    }

    public void SendMessage(IMessage message, Guid sender)
    {
        _messageQueue.Enqueue((message, sender));
        _queueSemaphore.Release();
    }

    
    public void Stop()
    {
        _isRunning = false;
        _queueSemaphore.Release();
    }

    public Task Start(CancellationToken cancellationToken) => MessageDistributionLoop(cancellationToken);

    private async Task MessageDistributionLoop(CancellationToken cancellationToken)
    {
        while (_isRunning && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _queueSemaphore.WaitAsync(cancellationToken);
                if (!_messageQueue.TryDequeue(out (IMessage message, Guid sender) message))
                    continue;

                await DistributeMessage(message.message, message.sender, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _isRunning = false;
            }
        }
    }

    private async Task DistributeMessage(IMessage message, Guid sender, CancellationToken cancellationToken)
    {
        if (message == null)
            throw new InvalidNetworkMessageException("Container contains faulty message");

        var messageType = message.GetType();
        
        if (!_messageListener.TryGetValue(messageType, out var listeners) || listeners.Count == 0)
        {
            DropMessage(message, sender);
            return;
        }

        foreach(var listener in listeners)
            await listener(message, sender, cancellationToken);
    }

    private static void DropMessage(IMessage message, Guid sender)
    {
        Console.WriteLine($"Warning: Dropped message. Type {message}, Sender {sender}");
    }
}