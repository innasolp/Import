using Import.Interfaces;
using Import.Settings.Interfaces;

namespace Import.Factory.Service;

public interface IImportServiceFactory
{
    IService Create(string name,
     IImportSource importSource,
     IImportSettingsProvider importSettingsProvider,
     IImportSettings importSettings);
}

public interface IAggregateServiceFactory
{
    IAggregateImportService Create(string name, IImportSource importSource,
     IImportSettingsProvider importSettingsProvider,
     IImportSettings importSettings,
     IEnumerable<IService> services,
      Func<IService, CancellationToken, Task>? executionTask = null);

    IAggregateImportService Create(string name,
    ISplittableImportSource importSource,
   IImportSettingsProvider importSettingsProvider,
   IImportSettings importSettings,
    Func<IImportSource, string>? getName = null,
    Func<IService, CancellationToken, Task>? executionTask = null);
}