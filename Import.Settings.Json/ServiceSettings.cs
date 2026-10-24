using Import.Settings.Interfaces;

namespace Import.Settings.Json;

public class ServiceSettings : IServiceSettings
{
    public string? ServiceTypeName { get; set; }
    public string? ImplementationTypeName { get; set; }
    public string? AssemblyPath { get; set; }
    public string? ServiceProviderPath { get; set; }
    public string? Value { get; set; }

    public static bool IsEmpty(IServiceSettings serviceSettings)
    {
        return string.IsNullOrEmpty(serviceSettings.AssemblyPath)
        && string.IsNullOrEmpty(serviceSettings.Value)
         && string.IsNullOrEmpty(serviceSettings.ImplementationTypeName)
          && string.IsNullOrEmpty(serviceSettings.ServiceProviderPath)
           && string.IsNullOrEmpty(serviceSettings.ServiceTypeName);
    }
}