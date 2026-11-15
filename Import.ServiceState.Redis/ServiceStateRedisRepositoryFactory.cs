namespace Import.ServiceState.Redis;

public class ServiceStateRedisRepositoryFactory : IServiceStateRepositoryFactory
{
    public static ServiceStateRedisRepository Create(string connectionString)
    {
        return new ServiceStateRedisRepository(connectionString);
    }

    IServiceStateRepository IServiceStateRepositoryFactory.Create(params object[] parameters)
    {
        if (parameters.Length == 0)
            throw new InvalidOperationException($"Parameter list can not be empty");

        if (parameters.FirstOrDefault(p => p is string) is not string connectionString || string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Invalid parameters");

        return Create(connectionString);
    }
}