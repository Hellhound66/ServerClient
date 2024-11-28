namespace Messages.Extensions;

public static class SemaphoreSlimExtensions
{
    public static async Task Lock(this SemaphoreSlim semaphoreSlim, Func<CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        await semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            await action(cancellationToken);
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }
    
    public static async Task<T> Lock<T>(this SemaphoreSlim semaphoreSlim, Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken)
    {
        await semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await action(cancellationToken);
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

}