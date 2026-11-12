using System.Text.Json.Nodes;
using Import.Settings.Interfaces;

namespace Import.Settings.Json;

public class JsonSettingsLoader : ISettingsLoader<JsonObject>
{
    public IImportSettings Load(JsonObject source)
    {
        return new ImportSettings(source);
    }
}