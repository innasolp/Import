using System.Text.Json;
using System.Text.Json.Nodes;
using Import.Settings.Interfaces;
using Import.Settings.Json;

namespace Import.Settings.Test;

public class JsonSettingsTest
{

    private readonly string _jsonContent;

    public JsonSettingsTest()
    {
        _jsonContent = File.ReadAllText("appsettings.json");
    }

    [Fact]

    public void LoadSettingsFromJsonSuccessWhenJsonIsValid()
    {
        using JsonDocument doc = JsonDocument.Parse(_jsonContent);

        Assert.True(doc.RootElement.TryGetProperty("OzonCategories", out JsonElement sectionElement));
        var jsonObject = JsonObject.Create(sectionElement);
        Assert.NotNull(jsonObject);
        var importSettings = new ImportSettings(jsonObject);
        IImportSettingsProvider importSettingsProvider = new ImportSettingsProvider();

        Assert.True(importSettings.ServiceSettings.ContainsKey("ImportService"));
        Assert.True(importSettings.ServiceSettings.ContainsKey("Loader"));
        Assert.True(importSettingsProvider.TryGetService<object[]>(importSettings, "LoadStages", out var collection));
        Assert.Equal(2, collection!.Length);
        Assert.False(importSettingsProvider.TryGetService<object>(importSettings, Guid.NewGuid().ToString(), out var _));
    }
}