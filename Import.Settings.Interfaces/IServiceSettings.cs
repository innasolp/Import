namespace Import.Settings.Interfaces;

public interface IServiceSettings
{
    string? ServiceTypeName { get; set; }

    string? ImplementationTypeName { get; set; }

    string? AssemblyPath { get; set; }

    string? ServiceProviderPath { get; set; }

    string? Value { get; set; }
}
