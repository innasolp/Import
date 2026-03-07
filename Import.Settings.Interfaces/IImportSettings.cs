using System.Collections;

namespace Import.Settings.Interfaces;

public interface IImportSettings
{
    IDictionary Services { get; }
}