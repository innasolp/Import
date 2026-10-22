using Import.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace Import.ServiceState.InMemory;

public class ServiceStateInMemoryRepository(IMemoryCache cache) : IServiceStateRepository
{
    private bool _isConnected = false;

    private readonly IMemoryCache _cache = cache;

    public bool IsConnected => _isConnected;

    private event AsyncEventHandler<ConnectedAsyncEventArgs>? ConnectedAsync;

    event AsyncEventHandler<ConnectedAsyncEventArgs> IServiceStateRepository.ConnectedAsync
    {
        add
        {
            ConnectedAsync += value;
        }

        remove
        {
            ConnectedAsync -= value;
        }
    }

    public Task<T?> Load<T>(byte[] key, CancellationToken cancellationToken = default)
    {
        var hexHash = Convert.ToHexString(key);
        var json = _cache.Get<string>(hexHash);
        if (json is null)
            return Task.FromResult<T?>(default);
        return Task.FromResult(JsonSerializer.Deserialize<T>(json));
    }

    public Task Save<T>(byte[] key, T value, CancellationToken cancellationToken = default)
    {
        var hexHash = Convert.ToHexString(key);
        var json = JsonSerializer.Serialize(value);
        var entry = _cache.CreateEntry(hexHash);
        entry.Value = json;
        entry.Size = 1;
        entry.SlidingExpiration = TimeSpan.FromHours(24);
        entry.Dispose();
        return Task.CompletedTask;
    }

    Task IServiceStateRepository.Close(CancellationToken cancellationToken)
    {
        return InvokeConnectedAsync(false, null, cancellationToken);
    }

    Task IServiceStateRepository.Connect(CancellationToken cancellationToken)
    {
        return InvokeConnectedAsync(true, null, cancellationToken);
    }

    protected async Task InvokeConnectedAsync(bool success, Exception? exception = null, CancellationToken cancellationToken = default)
    {
        _isConnected = success;

        var handlers = ConnectedAsync;
        if (handlers == null)
            return;

        var args = new ConnectedAsyncEventArgs(success, exception, cancellationToken);

        async Task HandlerTask(AsyncEventHandler<ConnectedAsyncEventArgs> handler)
        {
            await handler(this, args).ConfigureAwait(false);
        }
        ;

        var tasks = handlers.GetInvocationList()
            .OfType<AsyncEventHandler<ConnectedAsyncEventArgs>>()
            .Select(HandlerTask);

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public Task Remove(byte[] key, CancellationToken cancellationToken = default)
    {
        var hexHash = Convert.ToHexString(key);
        _cache.Remove(hexHash);
        return Task.CompletedTask;
    }
}