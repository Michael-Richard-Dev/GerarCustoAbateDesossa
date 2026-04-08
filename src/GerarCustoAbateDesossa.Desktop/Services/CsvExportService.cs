using System.Data;
using System.Text;

namespace GerarCustoAbateDesossa.Desktop.Services;

public sealed class CsvExportService : ICsvExportService
{
    public void ExportToCsv(DataTable dataTable, string filePath)
    {
        using var writer = new StreamWriter(filePath, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        var headers = dataTable.Columns
            .Cast<DataColumn>()
            .Select(column => EscapeCsv(column.ColumnName));
        writer.WriteLine(string.Join(';', headers));

        foreach (DataRow row in dataTable.Rows)
        {
            var values = dataTable.Columns
                .Cast<DataColumn>()
                .Select(column => EscapeCsv(row[column]?.ToString() ?? string.Empty));
            writer.WriteLine(string.Join(';', values));
        }
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains('"'))
        {
            value = value.Replace("\"", "\"\"");
        }

        return value.IndexOfAny([';', '"', '\r', '\n']) >= 0
            ? $"\"{value}\""
            : value;
    }
}
