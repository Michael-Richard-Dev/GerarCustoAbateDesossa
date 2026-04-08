using GerarCustoAbateDesossa.Infrastructure.Configuration;
using GerarCustoAbateDesossa.Infrastructure.Data;

namespace GerarCustoAbateDesossa.Desktop;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "CONFIG.INI");
            var databaseOptions = ConfigurationLoader.LoadDatabaseOptions(configPath);
            var providerResolver = new DbProviderFactoryResolver();
            var costDataService = new CostDataService(databaseOptions, providerResolver);

            System.Windows.Forms.Application.Run(new MainForm(costDataService));
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Falha ao iniciar a aplicacao.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                "Erro",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
