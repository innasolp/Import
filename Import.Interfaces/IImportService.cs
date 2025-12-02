namespace Import.Interfaces;

public interface IImportService
{
    Task Start(CancellationToken stoppingToken);

    string Name { get; }

    bool IsStarted { get; }
}
