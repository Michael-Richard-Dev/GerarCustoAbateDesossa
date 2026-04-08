namespace GerarCustoAbateDesossa.Domain;

public sealed record DatabaseOptions(
    string ProviderInvariantName,
    string ConnectionString,
    string? ProviderAssemblyPath,
    string? LibraryLocation,
    string? TnsAdmin);
