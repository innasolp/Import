namespace Import.Settings.Interfaces;

public interface ISettingsLoader<T>
{
    IImportSettings Load(T source);
}