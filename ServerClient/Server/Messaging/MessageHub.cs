using System.Collections.Concurrent;
using Messages.Contracts;
using Server.Contracts;
using Server.Exceptions;

namespace Server.Messaging;

internal class MessageHub : IMessageHub
{
    private readonly ConcurrentDictionary<Type, List<Func<IMessage, CancellationToken, Task>>> _messageListener = new();
    private readonly ConcurrentQueue<IMessage> _messageQueue = new();
    private readonly SemaphoreSlim _queueSemaphore = new(0);

    private bool _isRunning = true;

    public void RegisterListener<T>(Func<T, CancellationToken, Task> action) where T : IMessage
    {
        if (!_messageListener.TryGetValue(typeof(T), out var listenerList))
        {
            listenerList = [];
            _messageListener.TryAdd(typeof(T), listenerList);
        }

        listenerList.Add((message, token) => action((T)message, token));
    }

    public void PushMessage(IMessage message)
    {
        _messageQueue.Enqueue(message);
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
        while (_isRunning)
        {
            try
            {
                await _queueSemaphore.WaitAsync(cancellationToken);
                if (!_messageQueue.TryDequeue(out var messageContainer))
                    continue;

                await DistributeMessage(messageContainer, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _isRunning = false;
            }
        }
    }

    private async Task DistributeMessage(IMessage message, CancellationToken cancellationToken)
    {
        if (message == null)
            throw new InvalidNetworkMessageException("Container contains faulty message");

        if (!_messageListener.TryGetValue(message.GetType(), out var listeners) || listeners.Count == 0)
        {
            DropMessage(message);
            return;
        }

        foreach(var listener in listeners)
            await listener(message, cancellationToken);
    }

    private static void DropMessage(IMessage message)
    {
        Console.WriteLine("Warning: Dropped message.");
    }
}