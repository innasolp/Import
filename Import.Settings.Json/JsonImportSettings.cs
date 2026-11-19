using System.Collections;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using Import.Settings.Interfaces;

namespace Import.Settings.Json;

public class JsonImportSettings : IImportSettings
{  
    private readonly Dictionary<string, JsonElement> _properties = [];

    public JsonImportSettings(JsonObject json)
    {
        _properties = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json) ?? [];

        if (_properties.TryGetValue("Services", out var servicesJsonNode))
        {
            var serviceJsonNodeString = servicesJsonNode.ToString();
            if (!serviceJsonNodeString.TryDeserialize<Dictionary<string, JsonNode>>(out var services)
                || services is null)
                throw new InvalidOperationException("Failed to deserialize 'services' property.");
            foreach (var kvp in services)
                _services[kvp.Key] = kvp.Value;
            foreach (var serviceKey in _services.Keys)
            {
                var jsonString = _services[serviceKey].ToString();

                if (Helper.TryDeserialize<ServiceSettings>(jsonString, out var serviceSettings)
                        && Json.ServiceSettings.IsEmpty(serviceSettings) == false)
                    ServiceSettings.Add(serviceKey, serviceSettings);
            }
        }

        Services = new ReadOnlyDictionary<string, object>(_services);
        InternalServices = new FixedSizeDictionary<string, object>(_services);
    }

    internal IReadOnlyDictionary<string, JsonElement> Properties => _properties;

    public Dictionary<string, ServiceSettings> ServiceSettings { get; private set; } = [];

    private readonly Dictionary<string, object> _services = [];

    internal FixedSizeDictionary<string, object> InternalServices { get; private set; }

    internal ReadOnlyDictionary<string, object> Services { get; private set; }

    IDictionary IImportSettings.Services => Services;
}