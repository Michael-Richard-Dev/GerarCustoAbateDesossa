using GerarCustoAbateDesossa.Domain;

namespace GerarCustoAbateDesossa.Infrastructure.Configuration;

public static class ConfigurationLoader
{
    public static DatabaseOptions LoadDatabaseOptions(string configPath)
    {
        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException(
                $"Arquivo de configuracao nao encontrado: {configPath}",
                configPath);
        }

        var ini = IniFileReader.Load(configPath);

        var providerInvariantName = ini.GetValue("DATABASE", "ProviderInvariantName", "Oracle.ManagedDataAccess.Client");
        var providerAssemblyPath = ExpandOptionalPath(
            ini.GetValue("DATABASE", "ProviderAssemblyPath", string.Empty),
            configPath);
        var libraryLocation = ExpandOptionalPath(
            ini.GetValue("DATABASE", "LibraryLocation", ini.GetValue("CONFIG", "LibraryLocation", string.Empty)),
            configPath);
        var tnsAdmin = ExpandOptionalPath(
            ini.GetValue("DATABASE", "TnsAdmin", ini.GetValue("CONFIG", "TnsAdmin", string.Empty)),
            configPath);

        var connectionString = ini.GetValue("DATABASE", "ConnectionString", string.Empty);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var dataSource = ini.GetValue("DATABASE", "DataSource", string.Empty);
            var userId = ini.GetValue("DATABASE", "UserId", string.Empty);
            var password = ini.GetValue("DATABASE", "Password", string.Empty);

            if (!string.IsNullOrWhiteSpace(dataSource) && !string.IsNullOrWhiteSpace(userId))
            {
                connectionString = $"Data Source={dataSource};User Id={userId};Password={password};";
            }
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Defina a ConnectionString no arquivo CONFIG.INI para conectar ao Oracle.");
        }

        return new DatabaseOptions(
            providerInvariantName,
            connectionString,
            providerAssemblyPath,
            libraryLocation,
            tnsAdmin);
    }

    private static string? ExpandOptionalPath(string value, string configPath)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (Path.IsPathRooted(value))
        {
            return value;
        }

        var baseDirectory = Path.GetDirectoryName(configPath) ?? AppContext.BaseDirectory;
        return Path.GetFullPath(Path.Combine(baseDirectory, value));
    }
}
