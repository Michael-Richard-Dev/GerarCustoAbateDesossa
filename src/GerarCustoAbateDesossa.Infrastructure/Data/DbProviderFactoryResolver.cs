using System.Data.Common;
using System.Reflection;
using GerarCustoAbateDesossa.Domain;

namespace GerarCustoAbateDesossa.Infrastructure.Data;

public sealed class DbProviderFactoryResolver
{
    public DbProviderFactory Resolve(DatabaseOptions options)
    {
        EnsureNativeClientOnPath(options.LibraryLocation);
        LoadConfiguredAssembly(options.ProviderAssemblyPath);

        try
        {
            return DbProviderFactories.GetFactory(options.ProviderInvariantName);
        }
        catch (ArgumentException)
        {
            RegisterKnownProvider(options);
            return DbProviderFactories.GetFactory(options.ProviderInvariantName);
        }
    }

    private static void EnsureNativeClientOnPath(string? libraryLocation)
    {
        if (string.IsNullOrWhiteSpace(libraryLocation))
        {
            return;
        }

        if (!File.Exists(libraryLocation))
        {
            throw new FileNotFoundException(
                $"Nao foi possivel localizar a biblioteca nativa configurada: {libraryLocation}",
                libraryLocation);
        }

        var directory = Path.GetDirectoryName(libraryLocation);
        if (string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        var currentPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        if (!currentPath.Contains(directory, StringComparison.OrdinalIgnoreCase))
        {
            Environment.SetEnvironmentVariable("PATH", $"{directory};{currentPath}");
        }
    }

    private static void LoadConfiguredAssembly(string? providerAssemblyPath)
    {
        if (string.IsNullOrWhiteSpace(providerAssemblyPath))
        {
            return;
        }

        if (!File.Exists(providerAssemblyPath))
        {
            throw new FileNotFoundException(
                $"Nao foi possivel localizar o provider ADO.NET configurado: {providerAssemblyPath}",
                providerAssemblyPath);
        }

        Assembly.LoadFrom(providerAssemblyPath);
    }

    private static void RegisterKnownProvider(DatabaseOptions options)
    {
        if (options.ProviderInvariantName.Equals("Oracle.ManagedDataAccess.Client", StringComparison.OrdinalIgnoreCase))
        {
            var assembly = TryLoadOracleManagedAssembly(options.ProviderAssemblyPath);
            var factoryType = assembly.GetType("Oracle.ManagedDataAccess.Client.OracleClientFactory", throwOnError: true)
                ?? throw new InvalidOperationException("Nao foi possivel localizar OracleClientFactory.");

            var instance = factoryType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
            if (instance is not DbProviderFactory factory)
            {
                throw new InvalidOperationException("Nao foi possivel inicializar a fabrica do provider Oracle.");
            }

            DbProviderFactories.RegisterFactory(options.ProviderInvariantName, factory);
            return;
        }

        throw new InvalidOperationException(
            $"O provider '{options.ProviderInvariantName}' nao esta registrado. Configure 'ProviderAssemblyPath' com o driver correto.");
    }

    private static Assembly TryLoadOracleManagedAssembly(string? providerAssemblyPath)
    {
        if (!string.IsNullOrWhiteSpace(providerAssemblyPath))
        {
            return Assembly.LoadFrom(providerAssemblyPath);
        }

        try
        {
            return Assembly.Load(new AssemblyName("Oracle.ManagedDataAccess"));
        }
        catch (FileNotFoundException ex)
        {
            throw new InvalidOperationException(
                "Oracle.ManagedDataAccess nao foi encontrado. Informe o caminho do DLL em ProviderAssemblyPath no CONFIG.INI.",
                ex);
        }
    }
}
