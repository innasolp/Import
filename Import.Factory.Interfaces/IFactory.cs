using Import.Settings.Interfaces;

namespace Import.Factory.Interfaces;

public interface IFactory
{
    Type GetServiceType();
}

public interface IFactory<T> : IFactory
{
    T Create(IImportSettingsProvider importSettingsProvider, IImportSettings importSettings);
}