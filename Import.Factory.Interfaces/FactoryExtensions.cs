using Import.Settings.Interfaces;

namespace Import.Factory.Interfaces;

public static class FactoryExtensions
{
    private static string NormalizeTypeName(string name)
    {
        var backtickIndex = name.IndexOf('`');
        return backtickIndex >= 0 ? name[..backtickIndex] : name;
    }

    private static bool EqualsServiceTypeName(this Type actualType, string expected)
    {
        var actual = NormalizeTypeName(actualType.Name);

        return expected.Equals(actual, StringComparison.InvariantCultureIgnoreCase)
        || $"{expected}Factory".Equals(actual, StringComparison.InvariantCultureIgnoreCase)
        || $"{expected}Impl".Equals(actual, StringComparison.InvariantCultureIgnoreCase)
        || $"{expected}FactoryImpl".Equals(actual, StringComparison.InvariantCultureIgnoreCase);
    }

    private static T? GetFactoryByServiceSettings<T>(this IEnumerable<T> items, IServiceSettings serviceSettings, string sourceName)
     where T : IFactory
    {
        var expectedName = NormalizeTypeName(serviceSettings.ImplementationTypeName);

        return items.FirstOrDefault
          (i => i.GetServiceType().EqualsServiceTypeName(expectedName) ||
           i.GetType().EqualsServiceTypeName(expectedName));
    }

    public static T GetFactoryBySettings<T>(this IEnumerable<T> items, IImportSettingsProvider importSettingsProvider,
       IImportSettings importSettings, string key, string sourceName)
     where T : IFactory
    {
        if (!importSettingsProvider.TryGetServiceSettings(importSettings, key, out var serviceSettings)
        || string.IsNullOrEmpty(serviceSettings.ImplementationTypeName))
            throw new InvalidDataException($"No settings {key} for type {typeof(T).Name}  for service {sourceName}.");

        return items.GetFactoryByServiceSettings(serviceSettings, sourceName)
          ?? throw new InvalidDataException($"No service {key} of type {serviceSettings.ImplementationTypeName} for service {sourceName}.");
    }

    public static T GetItemBySettings<T>(this IEnumerable<T> items, IImportSettingsProvider importSettingsProvider,
     IImportSettings importSettings, string key, string sourceName)
    {
        if (!importSettingsProvider.TryGetServiceSettings(importSettings, key, out var serviceSettings)
        || string.IsNullOrEmpty(serviceSettings.ImplementationTypeName))
            throw new InvalidDataException($"No settings {key} for type {typeof(T).Name}  for service {sourceName}.");

        var expectedName = NormalizeTypeName(serviceSettings.ImplementationTypeName);

        return items.FirstOrDefault
          (i => NormalizeTypeName(i.GetType().Name).Equals(expectedName, StringComparison.InvariantCultureIgnoreCase))
          ?? throw new InvalidDataException($"No service {key} of type {serviceSettings.ImplementationTypeName} for service {sourceName}.");
    }

    public static T GetFactoryByImplementationName<T>(this IEnumerable<T> factories, string implementationTypeName, string sourceName)
    {
        var expectedName = NormalizeTypeName(implementationTypeName);

        return factories.FirstOrDefault
          (i => i.GetType().EqualsServiceTypeName(expectedName))
          ?? throw new InvalidDataException($"No service of type {implementationTypeName} for service {sourceName}.");
    }
}