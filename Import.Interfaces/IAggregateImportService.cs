namespace Import.Interfaces;

public interface IAggregateImportService : IService
{
    IEnumerable<(IService Service, bool Completed, bool? Success)> GetImportServices();

    Task Enqueue(IService service, CancellationToken cancellationToken = default);

    Task<bool> TryRemove(IService service, CancellationToken cancellationToken = default);
}