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

    public Dictionary<string, ServiceSettings> ServiceSettings { get; private set; } = [];

    public Dictionary<string, object> Services { get; private set; } = [];

    IDictionary IImportSettings.Services => Services;

    public T Get<T>(string key)
    {
        if (_properties.TryGetValue(key, out var jsonNode))
        {
            var jsonString = jsonNode.GetRawText();

            if (Helper.TryDeserialize<T>(jsonString, out var result))
                return result;

            if (Helper.TryParseDynamic<T>(jsonString, out result))
                return result;

            throw new Exception($"Invalid json for key: '{key}'");
        }

        throw new Exception($"Key not found: '{key}'");
    }

    public JsonObject GetJson(string key)
    {
        if (Services.TryGetValue(key, out var jsonNode))
        {
            return JsonSerializer.Deserialize<JsonObject>(jsonNode.ToString())!;// jsonNode;
        }

        throw new Exception($"Key not found: '{key}'");
    }

    public string GetString(string key)
    {
        if (_properties.TryGetValue(key, out var jsonNode))
        {
            return jsonNode.ToString();
        }

        throw new Exception($"Key not found: '{key}'");
    }

    public bool TryGet<T>(string key, out T? value)
    {
        value = default;

        if (_properties.TryGetValue(key, out var jsonNode))
        {
            var jsonString = jsonNode.GetRawText();

            if (Helper.TryDeserialize<T>(jsonString, out value))
                return true;

            if (Helper.TryParseDynamic<T>(jsonString, out value))
                return true;

            return false;
        }

        return false;
    }

    public bool TryGetJson(string key, out JsonObject? value)
    {
        value = default;
        if (Services.TryGetValue(key, out var service))
        {
            return Helper.TryDeserialize(service.ToString(), out value);
        }
        return false;
    }

    public bool TryGetService<T>(string key, out T? result) where T : class
    {
        result = default;
        if (Services.TryGetValue(key, out var service))
        {
            var jsonString = service.ToString();
            return Helper.TryDeserialize<T>(jsonString, out result);
        }
        return false;
    }

    public bool TryGetServiceSettings(string key, out IServiceSettings? result)
    {
        result = default;
        if (ServiceSettings.TryGetValue(key, out var service))
        {
            result = service;
            return true;
        }
        return false;
    }

    public bool TryGetString(string key, out string value)
    {
        value = default;
        if (_properties.TryGetValue(key, out var jsonNode))
        {
            value = jsonNode.ToString();
            return true;
        }

        return false;
    }

}