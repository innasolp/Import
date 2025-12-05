using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;

namespace Import.Factory.Interfaces;

public interface IImportServiceLogFactory
{
    ILogger<T> GetLogger<T>(ILogger<T> logger, string name, IImportSource shopModel, IShopImportSettings shopImportSettings);
}
