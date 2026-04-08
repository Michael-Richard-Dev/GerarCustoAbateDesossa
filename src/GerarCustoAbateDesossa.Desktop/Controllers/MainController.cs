using System.Data;
using GerarCustoAbateDesossa.Application;
using GerarCustoAbateDesossa.Desktop.Models;
using GerarCustoAbateDesossa.Desktop.Services;
using GerarCustoAbateDesossa.Desktop.Views;
using GerarCustoAbateDesossa.Domain;

namespace GerarCustoAbateDesossa.Desktop.Controllers;

public sealed class MainController
{
    private readonly IMainView _view;
    private readonly ICostDataService _costDataService;
    private readonly ICsvExportService _csvExportService;
    private readonly IReadOnlyList<CostTypeOption> _costTypes =
    [
        new(CostType.Abate, "Custo de Abate"),
        new(CostType.Desossa, "Custo de Desossa")
    ];

    private DataTable? _currentData;

    public MainController(IMainView view, ICostDataService costDataService, ICsvExportService csvExportService)
    {
        _view = view;
        _costDataService = costDataService;
        _csvExportService = csvExportService;

        _view.ViewLoaded += OnViewLoaded;
        _view.SearchRequested += OnSearchRequested;
        _view.ProcessRequested += OnProcessRequested;
        _view.ExportRequested += OnExportRequested;
    }

    private void OnViewLoaded(object? sender, EventArgs e)
    {
        _view.BindUnits(UnitCatalog.All);
        _view.BindCostTypes(_costTypes);
        _view.SetDateRange(DateTime.Today, DateTime.Today);
        _view.UpdateStatus("Pronto");
    }

    private async void OnSearchRequested(object? sender, EventArgs e)
    {
        await SearchAsync(manageBusyState: true);
    }

    private async void OnProcessRequested(object? sender, EventArgs e)
    {
        await ProcessAsync();
    }

    private void OnExportRequested(object? sender, EventArgs e)
    {
        ExportCurrentData();
    }

    private async Task SearchAsync(bool manageBusyState)
    {
        if (!TryGetSelectionContext(out var unit, out var costType, out var startDate, out var endDate))
        {
            return;
        }

        if (manageBusyState)
        {
            _view.SetBusy(true, "Carregando dados...");
        }

        try
        {
            var request = new CostSearchRequest(startDate, endDate, unit.Id, costType);
            _currentData = await Task.Run(() => _costDataService.LoadCosts(request));
            _view.SetData(_currentData);
            _view.UpdateStatus($"{_currentData.Rows.Count} registro(s) carregado(s).");
        }
        catch (Exception ex)
        {
            _view.ShowError("Falha ao carregar os dados.", ex);
        }
        finally
        {
            if (manageBusyState)
            {
                _view.SetBusy(false);
            }
        }
    }

    private async Task ProcessAsync()
    {
        if (!TryGetSelectionContext(out var unit, out var costType, out var startDate, out var endDate))
        {
            return;
        }

        _view.SetBusy(true, "Iniciando processamento...");

        try
        {
            var request = new CostProcessingRequest(startDate, endDate, unit.Id, costType);
            var result = await Task.Run(() =>
                _costDataService.ProcessCosts(request, ConfirmExistingRecords, _view.UpdateStatus));

            await SearchAsync(manageBusyState: false);

            if (result.Cancelled)
            {
                _view.UpdateStatus(
                    $"Processamento interrompido. Dias processados: {result.ProcessedDays}. Dias ignorados: {result.SkippedDays}.");
            }
            else
            {
                _view.UpdateStatus(
                    $"Processamento concluido. Dias processados: {result.ProcessedDays}. Dias ignorados: {result.SkippedDays}.");
            }
        }
        catch (Exception ex)
        {
            _view.ShowError("Falha ao processar os dados.", ex);
        }
        finally
        {
            _view.SetBusy(false);
        }
    }

    private void ExportCurrentData()
    {
        if (_currentData is null || _currentData.Rows.Count == 0)
        {
            _view.ShowInformation("Nao ha dados carregados para exportar.", "Exportacao");
            return;
        }

        var suggestedFileName =
            $"{(_view.SelectedCostType == CostType.Abate ? "custo_abate" : "custo_desossa")}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

        var filePath = _view.PromptExportFilePath(suggestedFileName);
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        try
        {
            _csvExportService.ExportToCsv(_currentData, filePath);
            _view.UpdateStatus($"Arquivo exportado para {filePath}");
            _view.ShowInformation("Exportacao concluida com sucesso.", "Exportacao");
        }
        catch (Exception ex)
        {
            _view.ShowError("Falha ao exportar o arquivo.", ex);
        }
    }

    private ExistingRecordDecision ConfirmExistingRecords(DateTime currentDate, int existingCount)
        => _view.ConfirmExistingRecords(currentDate, existingCount);

    private bool TryGetSelectionContext(
        out UnitOption unit,
        out CostType costType,
        out DateTime startDate,
        out DateTime endDate)
    {
        unit = _view.SelectedUnit ?? new UnitOption(0, string.Empty);
        costType = _view.SelectedCostType;
        startDate = _view.StartDate;
        endDate = _view.EndDate;

        if (_view.SelectedUnit is null)
        {
            _view.ShowWarning("Selecione uma unidade antes de continuar.");
            return false;
        }

        if (startDate > endDate)
        {
            _view.ShowWarning("A data inicial nao pode ser maior que a data final.");
            return false;
        }

        unit = _view.SelectedUnit;
        return true;
    }
}
