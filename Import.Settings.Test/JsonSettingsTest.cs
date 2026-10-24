using System.Text.Json;
using System.Text.Json.Nodes;
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

        Assert.True(importSettings.ServiceSettings.ContainsKey("ImportService"));
        Assert.True(importSettings.ServiceSettings.ContainsKey("Loader"));
        Assert.True(importSettings.TryGetService<object[]>("LoadStages", out var collection));
        Assert.Equal(2, collection!.Length);
        Assert.False(importSettings.TryGetService<object>(Guid.NewGuid().ToString(), out var _));
    }
}