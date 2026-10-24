using Import.Settings.Interfaces;

namespace Import.Factory.Interfaces;

public static class FactoryExtensions
{
    public static T GetFactoryBySettings<T>(this IEnumerable<T> items, IImportSettings importSettings, string key, string sourceName)
     where T : IFactory
    {
        if (!importSettings.TryGetServiceSettings(key, out var serviceSettings)
        || string.IsNullOrEmpty(serviceSettings.ImplementationTypeName))
            throw new InvalidDataException($"No settings for type {typeof(T).Name}  for service {sourceName}.");
        return items.FirstOrDefault
          (i => i.GetServiceType().Name.Equals(serviceSettings.ImplementationTypeName, StringComparison.InvariantCultureIgnoreCase) ||
           i.GetType().Name.Equals(serviceSettings.ImplementationTypeName, StringComparison.InvariantCultureIgnoreCase))
          ?? throw new InvalidDataException($"No service of type {serviceSettings.ImplementationTypeName} for service {sourceName}.");
    }

    public static T GetItemBySettings<T>(this IEnumerable<T> items, IImportSettings importSettings, string key, string sourceName)
    {
        if (!importSettings.TryGetServiceSettings(key, out var serviceSettings)
        || string.IsNullOrEmpty(serviceSettings.ImplementationTypeName))
            throw new InvalidDataException($"No settings for type {typeof(T).Name}  for service {sourceName}.");
        return items.FirstOrDefault
          (i => i.GetType().Name.Equals(serviceSettings.ImplementationTypeName, StringComparison.InvariantCultureIgnoreCase))
          ?? throw new InvalidDataException($"No service of type {serviceSettings.ImplementationTypeName} for service {sourceName}.");
    }
}