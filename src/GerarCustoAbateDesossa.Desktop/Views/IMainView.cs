using System.Data;
using GerarCustoAbateDesossa.Desktop.Models;
using GerarCustoAbateDesossa.Domain;

namespace GerarCustoAbateDesossa.Desktop.Views;

public interface IMainView
{
    event EventHandler? ViewLoaded;

    event EventHandler? SearchRequested;

    event EventHandler? ProcessRequested;

    event EventHandler? ExportRequested;

    DateTime StartDate { get; }

    DateTime EndDate { get; }

    UnitOption? SelectedUnit { get; }

    CostType SelectedCostType { get; }

    void BindUnits(IEnumerable<UnitOption> units);

    void BindCostTypes(IEnumerable<CostTypeOption> costTypes);

    void SetDateRange(DateTime startDate, DateTime endDate);

    void SetData(DataTable dataTable);

    void SetBusy(bool isBusy, string? statusText = null);

    void UpdateStatus(string text);

    void ShowError(string message, Exception? exception = null);

    void ShowWarning(string message, string title = "Validacao");

    void ShowInformation(string message, string title = "Informacao");

    ExistingRecordDecision ConfirmExistingRecords(DateTime currentDate, int existingCount);

    string? PromptExportFilePath(string suggestedFileName);
}
