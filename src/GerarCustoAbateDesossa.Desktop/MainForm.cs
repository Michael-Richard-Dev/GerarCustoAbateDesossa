using System.ComponentModel;
using System.Data;
using System.Text;
using GerarCustoAbateDesossa.Application;
using GerarCustoAbateDesossa.Domain;

namespace GerarCustoAbateDesossa.Desktop;

public partial class MainForm : Form
{
    private readonly ICostDataService? _costDataService;
    private DataTable? _currentData;

    public MainForm()
    {
        InitializeComponent();
        ConfigureScreen();
    }

    public MainForm(ICostDataService costDataService) : this()
    {
        _costDataService = costDataService;
        UpdateStatus("Pronto");
    }

    private void ConfigureScreen()
    {
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
        {
            return;
        }

        Font = new Font("Segoe UI", 9F);

        cbUnidade.DisplayMember = nameof(UnitOption.DisplayName);
        cbUnidade.ValueMember = nameof(UnitOption.Id);
        cbUnidade.DataSource = UnitCatalog.All.ToList();

        cbTipo.Items.Add("Custo de Abate");
        cbTipo.Items.Add("Custo de Desossa");
        cbTipo.SelectedIndex = 0;

        dtpInicial.Value = DateTime.Today;
        dtpFinal.Value = DateTime.Today;
    }

    private CostType SelectedCostType => cbTipo.SelectedIndex == 0 ? CostType.Abate : CostType.Desossa;

    private UnitOption SelectedUnit => (UnitOption)cbUnidade.SelectedItem!;

    private async void btnPesquisar_Click(object? sender, EventArgs e)
    {
        await LoadDataAsync(manageBusyState: true);
    }

    private async void btnProcessar_Click(object? sender, EventArgs e)
    {
        if (!TryValidateInputs())
        {
            return;
        }

        if (_costDataService is null)
        {
            ShowError("O servico de dados nao foi inicializado. Verifique o CONFIG.INI.");
            return;
        }

        SetBusy(true, "Iniciando processamento...");

        try
        {
            var request = new CostProcessingRequest(
                dtpInicial.Value.Date,
                dtpFinal.Value.Date,
                SelectedUnit.Id,
                SelectedCostType);

            var result = await Task.Run(() =>
                _costDataService.ProcessCosts(request, ResolveExistingRecords, UpdateStatus));

            await LoadDataAsync(manageBusyState: false);

            if (result.Cancelled)
            {
                UpdateStatus($"Processamento interrompido. Dias processados: {result.ProcessedDays}. Dias ignorados: {result.SkippedDays}.");
            }
            else
            {
                UpdateStatus($"Processamento concluido. Dias processados: {result.ProcessedDays}. Dias ignorados: {result.SkippedDays}.");
            }
        }
        catch (Exception ex)
        {
            ShowError("Falha ao processar os dados.", ex);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void btnExportar_Click(object? sender, EventArgs e)
    {
        if (_currentData is null || _currentData.Rows.Count == 0)
        {
            MessageBox.Show(
                this,
                "Nao ha dados carregados para exportar.",
                "Exportacao",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        using var saveFileDialog = new SaveFileDialog
        {
            Filter = "Arquivo CSV (*.csv)|*.csv",
            FileName = $"{(SelectedCostType == CostType.Abate ? "custo_abate" : "custo_desossa")}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
        };

        if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            ExportToCsv(_currentData, saveFileDialog.FileName);
            UpdateStatus($"Arquivo exportado para {saveFileDialog.FileName}");

            MessageBox.Show(
                this,
                "Exportacao concluida com sucesso.",
                "Exportacao",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            ShowError("Falha ao exportar o arquivo.", ex);
        }
    }

    private async Task LoadDataAsync(bool manageBusyState)
    {
        if (!TryValidateInputs())
        {
            return;
        }

        if (_costDataService is null)
        {
            ShowError("O servico de dados nao foi inicializado. Verifique o CONFIG.INI.");
            return;
        }

        if (manageBusyState)
        {
            SetBusy(true, "Carregando dados...");
        }

        try
        {
            var request = new CostSearchRequest(
                dtpInicial.Value.Date,
                dtpFinal.Value.Date,
                SelectedUnit.Id,
                SelectedCostType);

            _currentData = await Task.Run(() => _costDataService.LoadCosts(request));
            gridDados.DataSource = _currentData;
            UpdateStatus($"{_currentData.Rows.Count} registro(s) carregado(s).");
        }
        catch (Exception ex)
        {
            ShowError("Falha ao carregar os dados.", ex);
        }
        finally
        {
            if (manageBusyState)
            {
                SetBusy(false);
            }
        }
    }

    private ExistingRecordDecision ResolveExistingRecords(DateTime currentDate, int existingCount)
    {
        if (InvokeRequired)
        {
            return (ExistingRecordDecision)Invoke(
                new Func<DateTime, int, ExistingRecordDecision>(ResolveExistingRecords),
                currentDate,
                existingCount)!;
        }

        var result = MessageBox.Show(
            this,
            $"Ja existem {existingCount} registro(s) no dia {currentDate:dd/MM/yyyy}. Deseja refazer?",
            "Atencao",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question);

        return result switch
        {
            DialogResult.Yes => ExistingRecordDecision.Replace,
            DialogResult.No => ExistingRecordDecision.SkipDay,
            _ => ExistingRecordDecision.CancelProcessing
        };
    }

    private bool TryValidateInputs()
    {
        if (dtpInicial.Value.Date > dtpFinal.Value.Date)
        {
            MessageBox.Show(
                this,
                "A data inicial nao pode ser maior que a data final.",
                "Validacao",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return false;
        }

        return true;
    }

    private void SetBusy(bool isBusy, string? statusText = null)
    {
        panelFiltros.Enabled = !isBusy;
        btnExportar.Enabled = !isBusy;
        UseWaitCursor = isBusy;

        if (!string.IsNullOrWhiteSpace(statusText))
        {
            UpdateStatus(statusText);
        }
    }

    private void UpdateStatus(string text)
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(new Action<string>(UpdateStatus), text);
            return;
        }

        lblStatus.Text = text;
    }

    private void ShowError(string message, Exception? exception = null)
    {
        var fullMessage = exception is null
            ? message
            : $"{message}{Environment.NewLine}{Environment.NewLine}{exception.Message}";

        MessageBox.Show(
            this,
            fullMessage,
            "Erro",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }

    private static void ExportToCsv(DataTable dataTable, string filePath)
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
