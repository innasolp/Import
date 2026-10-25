using System.Collections;
using System.Text.Json.Nodes;
using Microsoft.VisualBasic;

namespace Import.Settings.Interfaces;

public interface IImportSettings
{
    IDictionary Services { get; }

    bool TryGet<T>(string key, out T? value);

    bool TryGetString(string key, out string? value);

    bool TryGetJson(string key, out JsonObject? value);

    bool TryGetService<T>(string key, out T? serviceSettings)
    where T : class;

    bool TryGetServiceSettings(string key, out IServiceSettings? serviceSettings);
}