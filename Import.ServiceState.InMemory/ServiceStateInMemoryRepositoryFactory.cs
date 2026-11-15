using Microsoft.Extensions.Caching.Memory;

namespace Import.ServiceState.InMemory;

public class ServiceStateInMemoryRepositoryFactory(IMemoryCache cache) : IServiceStateRepositoryFactory
{
    public ServiceStateInMemoryRepository Create()
    {
        return new ServiceStateInMemoryRepository(cache);
    }

    IServiceStateRepository IServiceStateRepositoryFactory.Create(params object[] parameters)
    {
        return new ServiceStateInMemoryRepository(cache);
    }
}