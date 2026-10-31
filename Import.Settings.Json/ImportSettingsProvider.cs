using System.Text.Json;
using System.Text.Json.Nodes;
using Import.Settings.Interfaces;

namespace Import.Settings.Json;

public class ImportSettingsProvider : IImportSettingsProvider
{
    bool IImportSettingsProvider.TryGet<T>(IImportSettings importSettings, string key, out T? value) where T : default
    {
        if (importSettings is ImportSettings jsonImportSettings)
            return TryGet(jsonImportSettings, key, out value);
        throw new ArgumentException($"Expected {nameof(ImportSettings)} but got {importSettings.GetType().FullName}");
    }

    bool IImportSettingsProvider.TryGetJson(IImportSettings importSettings, string key, out JsonObject? value)
    {
        if (importSettings is ImportSettings jsonImportSettings)
            return TryGetJson(jsonImportSettings, key, out value);
        throw new ArgumentException($"Expected {nameof(ImportSettings)} but got {importSettings.GetType().FullName}");
    }

    bool IImportSettingsProvider.TryGetService<T>(IImportSettings importSettings, string key, out T? service)
        where T : class
    {
        if (importSettings is ImportSettings jsonImportSettings)
            return TryGetService(jsonImportSettings, key, out service);
        throw new ArgumentException($"Expected {nameof(ImportSettings)} but got {importSettings.GetType().FullName}");
    }

    bool IImportSettingsProvider.TryGetServiceSettings(IImportSettings importSettings, string key, out IServiceSettings? serviceSettings)
    {
        if (importSettings is ImportSettings jsonImportSettings)
            return TryGetServiceSettings(jsonImportSettings, key, out serviceSettings);
        throw new ArgumentException($"Expected {nameof(ImportSettings)} but got {importSettings.GetType().FullName}");
    }

    bool IImportSettingsProvider.TryGetString(IImportSettings importSettings, string key, out string? value)
    {
        if (importSettings is ImportSettings jsonImportSettings)
            return TryGetString(jsonImportSettings, key, out value);
        throw new ArgumentException($"Expected {nameof(ImportSettings)} but got {importSettings.GetType().FullName}");
    }

    public static T Get<T>(ImportSettings importSettings, string key)
    {
        if (importSettings.Properties.TryGetValue(key, out var jsonNode))
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

    public static JsonObject GetJson(ImportSettings importSettings, string key)
    {
        if (importSettings.Services.TryGetValue(key, out var jsonNode))
        {
            return JsonSerializer.Deserialize<JsonObject>(jsonNode.ToString())!;// jsonNode;
        }

        throw new Exception($"Key not found: '{key}'");
    }

    public static string GetString(ImportSettings importSettings, string key)
    {
        if (importSettings.Properties.TryGetValue(key, out var jsonNode))
        {
            return jsonNode.ToString();
        }

        throw new Exception($"Key not found: '{key}'");
    }

    public static bool TryGet<T>(ImportSettings importSettings, string key, out T? value)
    {
        value = default;

        if (importSettings.Properties.TryGetValue(key, out var jsonNode))
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

    public static bool TryGetJson(ImportSettings importSettings, string key, out JsonObject? value)
    {
        value = default;
        if (importSettings.Services.TryGetValue(key, out var service))
        {
            return Helper.TryDeserialize(service.ToString(), out value);
        }
        return false;
    }

    public static bool TryGetService<T>(ImportSettings importSettings, string key, out T? result) where T : class
    {
        result = default;
        if (importSettings.Services.TryGetValue(key, out var service))
        {
            var jsonString = service.ToString();
            return Helper.TryDeserialize<T>(jsonString, out result);
        }
        return false;
    }

    public static bool TryGetServiceSettings(ImportSettings importSettings, string key, out IServiceSettings? result)
    {
        result = default;
        if (importSettings.ServiceSettings.TryGetValue(key, out var service))
        {
            result = service;
            return true;
        }
        return false;
    }

    public static bool TryGetString(ImportSettings importSettings, string key, out string? value)
    {
        value = default;
        if (importSettings.Properties.TryGetValue(key, out var jsonNode))
        {
            value = jsonNode.ToString();
            return true;
        }

        return false;
    }
}