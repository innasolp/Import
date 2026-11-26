using Import.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System.Resources;
using Xunit.Abstractions;

namespace Import.Service.Test.Infrastructure;

public abstract class ImportServiceTest<TLoadData, TService, TLogger>(ITestOutputHelper outputHelper)
    where TService:ImportService<TLoadData>
    where TLogger:class, ILogger
{
    protected readonly ITestOutputHelper _outputHelper = outputHelper;

    protected ResourceManager LogResourceManager { get; } = new ResourceManager("Import.Service.LogMessages",
                               typeof(ImportService<TLoadData>).Assembly);
    protected ResourceManager MessagesResourceManager { get; } = new ResourceManager("Import.Service.Messages",
                               typeof(ImportService<TLoadData>).Assembly);

    protected Mock<TLogger> LoggerMock { get; } = new Mock<TLogger>();

    protected Mock<ILoaderService<TLoadData>> LoaderMock { get; } = new Mock<ILoaderService<TLoadData>>();

    protected abstract TService CreateService(string name);
}
