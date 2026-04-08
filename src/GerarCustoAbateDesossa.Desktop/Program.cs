using GerarCustoAbateDesossa.Desktop.Controllers;
using GerarCustoAbateDesossa.Desktop.Services;
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
            var costDataService = new CostDataService(databaseOptions);
            var csvExportService = new CsvExportService();
            var mainForm = new MainForm();
            _ = new MainController(mainForm, costDataService, csvExportService);

            System.Windows.Forms.Application.Run(mainForm);
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
