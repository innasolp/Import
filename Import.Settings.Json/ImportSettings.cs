using System.Collections;
using System.Text.Json;
using System.Text.Json.Nodes;
using Import.Settings.Interfaces;

namespace Import.Settings.Json;

public class ImportSettings : IImportSettings
{
    private readonly Dictionary<string, JsonElement> _properties = [];

    public ImportSettings(JsonObject json)
    {
        _properties = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json) ?? [];

        if (_properties.TryGetValue(nameof(Services), out var servicesJsonNode))
        {
            var serviceJsonNodeString = servicesJsonNode.ToString();
            if (!serviceJsonNodeString.TryDeserialize<Dictionary<string, object>>(out var services)
                || services is null)
                throw new InvalidOperationException("Failed to deserialize 'services' property.");
            Services = services;
            foreach (var serviceKey in Services.Keys)
            {
                var jsonString = Services[serviceKey].ToString();

                if (Helper.TryDeserialize<ServiceSettings>(jsonString, out var serviceSettings)
                        && Json.ServiceSettings.IsEmpty(serviceSettings) == false)
                    ServiceSettings.Add(serviceKey, serviceSettings);
            }
        }
    }

    public Dictionary<string, JsonElement> Properties => _properties;

    public Dictionary<string, ServiceSettings> ServiceSettings { get; private set; } = [];

    public Dictionary<string, object> Services { get; private set; } = [];

    IDictionary IImportSettings.Services => Services;
}