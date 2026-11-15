using Import.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace Import.ServiceState.Redis;

public class ServiceStateRedisRepository(string connectionString) : IServiceStateRepository
{
    public bool IsConnected => _redis?.IsConnected == true && _database != null;

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

    private ConnectionMultiplexer? _redis;

    private IDatabase? _database;

    public async Task Close(CancellationToken cancellationToken = default)
    {
        if (_redis != null)
        {
            try
            {
                await _redis.CloseAsync();
                await InvokeConnectedAsync(false, null, cancellationToken);
            }
            catch (Exception e)
            {
                await InvokeConnectedAsync(_redis.IsConnected, e, cancellationToken);
                throw;
            }
        }
    }

    public async Task Connect(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_redis != null)
                await _redis.DisposeAsync();

            _redis = await ConnectionMultiplexer.ConnectAsync(connectionString);
            _database = _redis.GetDatabase();
            await InvokeConnectedAsync(_redis.IsConnected, null, cancellationToken);
        }
        catch (Exception e)
        {
            await InvokeConnectedAsync(_redis?.IsConnected == true, e, cancellationToken);
            throw;
        }
    }

    protected async Task InvokeConnectedAsync(bool success, Exception? exception = null, CancellationToken cancellationToken = default)
    {
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

    public async ValueTask DisposeAsync()
    {
        await Close();

        if (_redis != null)
            await _redis.DisposeAsync();
    }

    public async Task<T?> Load<T>(byte[] key, CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
            throw new InvalidOperationException("Redis service state repository is not connected.");

        var json = await _database!.StringGetAsync(key);
        if (!json.HasValue)
            return default;

        return JsonSerializer.Deserialize<T>((string)json!);
    }

    public Task Save<T>(byte[] key, T value, CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
            throw new InvalidOperationException("Redis service state repository is not connected.");

        var json = JsonSerializer.Serialize(value);
        return _database!.StringSetAsync(key, json, TimeSpan.FromHours(24));
    }

    public Task Remove(byte[] key, CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
            throw new InvalidOperationException("Redis service state repository is not connected.");

        return _database!.KeyDeleteAsync(key);
    }
}