using System.Text.Json.Nodes;

namespace Import.Settings.Interfaces;

public interface IImportSettingsProvider
{
    bool TryGet<T>(IImportSettings importSettings, string key, out T? value);

    bool TryGetString(IImportSettings importSettings, string key, out string? value);

    bool TryGetJson(IImportSettings importSettings, string key, out JsonObject? value);

    bool TryGetService<T>(IImportSettings importSettings, string key, out T? serviceSettings)
    where T : class;

    bool TryGetServiceSettings(IImportSettings importSettings, string key, out IServiceSettings? serviceSettings);
}