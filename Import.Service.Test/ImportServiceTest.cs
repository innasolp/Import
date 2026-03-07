using Import.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System.Resources;
using Xunit.Abstractions;

namespace Import.Service.Test;

public abstract class ImportServiceTest<TService, TLogger>(ITestOutputHelper outputHelper)
    where TService:ImportService
    where TLogger:class, ILogger
{
    protected readonly ITestOutputHelper _outputHelper = outputHelper;

    protected ResourceManager LogResourceManager { get; } = new ResourceManager("Import.Service.LogMessages",
                               typeof(ImportService).Assembly);
    protected ResourceManager MessagesResourceManager { get; } = new ResourceManager("Import.Service.Messages",
                               typeof(ImportService).Assembly);

    protected Mock<TLogger> LoggerMock { get; } = new Mock<TLogger>();

    protected Mock<ILoaderService> LoaderMock { get; } = new Mock<ILoaderService>();

    protected abstract TService CreateService(string name);
}
