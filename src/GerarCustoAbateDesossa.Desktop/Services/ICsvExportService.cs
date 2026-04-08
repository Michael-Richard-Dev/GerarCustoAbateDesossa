using System.Data;

namespace GerarCustoAbateDesossa.Desktop.Services;

public interface ICsvExportService
{
    void ExportToCsv(DataTable dataTable, string filePath);
}
