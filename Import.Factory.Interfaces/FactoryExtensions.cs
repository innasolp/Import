using Import.Settings.Interfaces;

namespace Import.Factory.Interfaces;

public static class FactoryExtensions
{
    private static string NormalizeTypeName(string name)
    {
        var backtickIndex = name.IndexOf('`');
        return backtickIndex >= 0 ? name[..backtickIndex] : name;
    }

    public static T GetFactoryBySettings<T>(this IEnumerable<T> items, IImportSettings importSettings, string key, string sourceName)
     where T : IFactory
    {
        if (!importSettings.TryGetServiceSettings(key, out var serviceSettings)
        || string.IsNullOrEmpty(serviceSettings.ImplementationTypeName))
            throw new InvalidDataException($"No settings for type {typeof(T).Name}  for service {sourceName}.");

        var expectedName = NormalizeTypeName(serviceSettings.ImplementationTypeName);

        return items.FirstOrDefault
          (i => NormalizeTypeName(i.GetServiceType().Name).Equals(expectedName, StringComparison.InvariantCultureIgnoreCase) ||
           NormalizeTypeName(i.GetType().Name).Equals(expectedName, StringComparison.InvariantCultureIgnoreCase))
          ?? throw new InvalidDataException($"No service of type {serviceSettings.ImplementationTypeName} for service {sourceName}.");
    }

    public static T GetItemBySettings<T>(this IEnumerable<T> items, IImportSettings importSettings, string key, string sourceName)
    {
        if (!importSettings.TryGetServiceSettings(key, out var serviceSettings)
        || string.IsNullOrEmpty(serviceSettings.ImplementationTypeName))
            throw new InvalidDataException($"No settings for type {typeof(T).Name}  for service {sourceName}.");

        var expectedName = NormalizeTypeName(serviceSettings.ImplementationTypeName);

        return items.FirstOrDefault
          (i => NormalizeTypeName(i.GetType().Name).Equals(expectedName, StringComparison.InvariantCultureIgnoreCase))
          ?? throw new InvalidDataException($"No service of type {serviceSettings.ImplementationTypeName} for service {sourceName}.");
    }
}