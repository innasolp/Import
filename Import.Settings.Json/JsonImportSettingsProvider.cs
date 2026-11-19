using System.Text.Json;
using System.Text.Json.Nodes;
using Import.Settings.Interfaces;

namespace Import.Settings.Json;

public class JsonImportSettingsProvider : IImportSettingsProvider
{
    private static bool TryLoadJsonFromFile(JsonNode jsonNode, out FileSettings? fileSettings, out JsonObject? result)
    {
        result = default;
        fileSettings = default;

        var jsonObject = jsonNode as JsonObject;
        if (jsonObject == null)
            return false;

        var jsonString = jsonObject.ToString();

        if (Helper.TryDeserialize(jsonString, out fileSettings)
            && fileSettings != null
            && !string.IsNullOrEmpty(fileSettings.FilePath))
        {
            try
            {
                var fileContent = File.ReadAllText(fileSettings.FilePath);
                if (Helper.TryDeserialize<JsonObject>(fileContent, out var parsedFileJson)
                    && parsedFileJson != null)
                {
                    result = parsedFileJson;
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        return false;
    }

    private static bool IsFileReference(FileSettings? fileSettings)
        => fileSettings != null && !string.IsNullOrEmpty(fileSettings.FilePath);

    private static bool TryLoadFromFile<T>(JsonNode jsonNode, out FileSettings? fileSettings, out JsonObject? parsedFileJson, out T? result)
    {
        result = default;

        if (TryLoadJsonFromFile(jsonNode, out fileSettings, out parsedFileJson) && parsedFileJson != null)
        {
            var fileJsonString = parsedFileJson.ToString();
            if (Helper.TryDeserialize(fileJsonString, out result))
                return true;
            if (Helper.TryParseDynamic(fileJsonString, out result))
                return true;
        }

        return false;
    }

    private static bool TryLoadFromCache<T>(Tuple<JsonObject, FileSettings, JsonObject> tuple, out T? result)
    {
        result = default;

        var jsonString = tuple.Item3.ToString();
        if (Helper.TryDeserialize(jsonString, out result))
            return true;
        if (Helper.TryParseDynamic(jsonString, out result))
            return true;

        return false;
    }

    bool IImportSettingsProvider.TryGet<T>(IImportSettings importSettings, string key, out T? value) where T : default
    {
        if (importSettings is JsonImportSettings jsonImportSettings)
            return TryGet(jsonImportSettings, key, out value);
        throw new ArgumentException($"Expected {nameof(JsonImportSettings)} but got {importSettings.GetType().FullName}");
    }

    bool IImportSettingsProvider.TryGetJson(IImportSettings importSettings, string key, out JsonObject? value)
    {
        if (importSettings is JsonImportSettings jsonImportSettings)
            return TryGetJson(jsonImportSettings, key, out value);
        throw new ArgumentException($"Expected {nameof(JsonImportSettings)} but got {importSettings.GetType().FullName}");
    }

    bool IImportSettingsProvider.TryGetService<T>(IImportSettings importSettings, string key, out T? service)
        where T : class
    {
        if (importSettings is JsonImportSettings jsonImportSettings)
            return TryGetService(jsonImportSettings, key, out service);
        throw new ArgumentException($"Expected {nameof(JsonImportSettings)} but got {importSettings.GetType().FullName}");
    }

    bool IImportSettingsProvider.TryGetServiceSettings(IImportSettings importSettings, string key, out IServiceSettings? serviceSettings)
    {
        if (importSettings is JsonImportSettings jsonImportSettings)
            return TryGetServiceSettings(jsonImportSettings, key, out serviceSettings);
        throw new ArgumentException($"Expected {nameof(JsonImportSettings)} but got {importSettings.GetType().FullName}");
    }

    bool IImportSettingsProvider.TryGetString(IImportSettings importSettings, string key, out string? value)
    {
        if (importSettings is JsonImportSettings jsonImportSettings)
            return TryGetString(jsonImportSettings, key, out value);
        throw new ArgumentException($"Expected {nameof(JsonImportSettings)} but got {importSettings.GetType().FullName}");
    }

    public static T Get<T>(JsonImportSettings importSettings, string key)
    {
        if (TryGet(importSettings, key, out T? result) && result != null)
            return result;

        throw new Exception($"Key not found or deserialization failed: '{key}'");
    }

    public static JsonObject GetJson(JsonImportSettings importSettings, string key)
    {
        if (TryGetJson(importSettings, key, out var jsonNode) && jsonNode != null)
            return jsonNode;

        throw new Exception($"Key not found: '{key}'");
    }

    public static string GetString(JsonImportSettings importSettings, string key)
    {
        if (importSettings.Properties.TryGetValue(key, out var jsonNode))
        {
            return jsonNode.ToString();
        }

        throw new Exception($"Key not found: '{key}'");
    }

    public static bool TryGet<T>(JsonImportSettings importSettings, string key, out T? value)
    {
        value = default;

        if (importSettings.Properties.TryGetValue(key, out var jsonNode))
        {
            var jsonString = jsonNode.GetRawText();

            if (Helper.TryDeserialize(jsonString, out value))
                return true;

            if (Helper.TryParseDynamic(jsonString, out value))
                return true;
        }

        if (importSettings.InternalServices.TryGetValue(key, out var serviceValue))
        {
            if (serviceValue is Tuple<JsonObject, FileSettings, JsonObject> tuple)
                return TryLoadFromCache(tuple, out value);

            if (serviceValue is JsonNode jsonNodeSvc && TryLoadFromFile(jsonNodeSvc, out var fileSettings, out var parsedFileJson, out value))
            {
                importSettings.InternalServices[key] = Tuple.Create(jsonNodeSvc.AsObject(), fileSettings, parsedFileJson);
                return true;
            }
        }

        return false;
    }

    public static bool TryGetJson(JsonImportSettings importSettings, string key, out JsonObject? value)
    {
        value = default;

        if (importSettings.Services.TryGetValue(key, out var service) && service != null)
        {
            if (service is JsonNode serviceNode)
            {
                if (TryLoadJsonFromFile(serviceNode, out var fileSettings, out value) && value != null)
                {
                    importSettings.InternalServices[key] = Tuple.Create(serviceNode.AsObject(), fileSettings!, value);
                    return true;
                }

                // A failed file reference must not be returned as an inline JSON object.
                if (IsFileReference(fileSettings))
                    return false;
            }

            if (Helper.TryDeserialize(service.ToString(), out value) && value != null)
                return true;
        }

        if (importSettings.InternalServices.TryGetValue(key, out var serviceValue))
        {
            if (serviceValue is Tuple<JsonObject, FileSettings, JsonObject> tuple)
            {
                value = tuple.Item3;
                return true;
            }

            if (serviceValue is JsonNode jsonNodeSvc && TryLoadJsonFromFile(jsonNodeSvc, out var fileSettings, out value))
            {
                importSettings.InternalServices[key] = Tuple.Create(jsonNodeSvc.AsObject(), fileSettings, value);
                return true;
            }
        }

        return false;
    }

    public static bool TryGetService<T>(JsonImportSettings importSettings, string key, out T? result) where T : class
    {
        result = default;

        if (importSettings.InternalServices.TryGetValue(key, out var serviceValue))
        {
            if (serviceValue is Tuple<JsonObject, FileSettings, JsonObject> tuple)
                return TryLoadFromCache(tuple, out result);

            if (serviceValue is JsonNode jsonNodeSvc)
            {
                if (TryLoadFromFile(jsonNodeSvc, out var fileSettings, out var parsedFileJson, out result))
                {
                    importSettings.InternalServices[key] = Tuple.Create(jsonNodeSvc.AsObject(), fileSettings!, parsedFileJson!);
                    return true;
                }

                // Do not deserialize a file descriptor when its file could not be loaded.
                if (IsFileReference(fileSettings))
                    return false;
            }
        }

        if (importSettings.Services.TryGetValue(key, out var service) && service != null)
        {
            var jsonString = service.ToString();
            if (Helper.TryDeserialize(jsonString!, out result))
                return true;
        }

        return false;
    }

    public static bool TryGetServiceSettings(JsonImportSettings importSettings, string key, out IServiceSettings? result)
    {
        result = default;

        if (importSettings.InternalServices.TryGetValue(key, out var serviceValue))
        {
            if (serviceValue is Tuple<JsonObject, FileSettings, JsonObject> tuple
                && TryLoadFromCache<ServiceSettings>(tuple, out var serviceSettings)
                 && serviceSettings != null)
            {
                result = serviceSettings;
                return true;
            }

            if (serviceValue is JsonNode jsonNodeSvc
              && TryLoadFromFile(jsonNodeSvc, out var fileSettings, out var parsedFileJson, out serviceSettings)
              && serviceSettings != null)
            {
                importSettings.InternalServices[key] = Tuple.Create(jsonNodeSvc.AsObject(), fileSettings, parsedFileJson);
                result = serviceSettings;
                return true;
            }
        }

        if (importSettings.ServiceSettings.TryGetValue(key, out var service))
        {
            result = service;
            return true;
        }

        return false;
    }

    public static bool TryGetString(JsonImportSettings importSettings, string key, out string? value)
    {
        value = default;
        if (importSettings.Properties.TryGetValue(key, out var jsonNode))
        {
            value = jsonNode.ToString();
            return true;
        }

        return false;
    }

    public static bool TryGet<T>(JsonImportSettings importSettings, out T? value, out Exception? exception)
    {
        value = default;
        exception = null;
        try
        {
            var json = JsonSerializer.Serialize(importSettings.Properties);
            if (Helper.TryDeserialize(json, out value, out exception))
                return true;
            return false;
        }
        catch (Exception ex)
        {
            value = default;
            exception = ex;
            return false;
        }
    }

    bool IImportSettingsProvider.TryGet<T>(IImportSettings importSettings, out T? value, out Exception? exception) where T : default
    {
        if (importSettings is JsonImportSettings jsonImportSettings)
            return TryGet(jsonImportSettings, out value, out exception);
        throw new ArgumentException($"Expected {nameof(JsonImportSettings)} but got {importSettings.GetType().FullName}");
    }
}
