using System.Data;
using GerarCustoAbateDesossa.Desktop.Models;
using GerarCustoAbateDesossa.Desktop.Views;
using GerarCustoAbateDesossa.Domain;

namespace GerarCustoAbateDesossa.Desktop;

public partial class MainForm : Form, IMainView
{
    public event EventHandler? ViewLoaded;

    public event EventHandler? SearchRequested;

    public event EventHandler? ProcessRequested;

    public event EventHandler? ExportRequested;

    public MainForm()
    {
        InitializeComponent();
        ConfigureScreen();
    }

    public DateTime StartDate => dtpInicial.Value.Date;

    public DateTime EndDate => dtpFinal.Value.Date;

    public UnitOption? SelectedUnit => cbUnidade.SelectedItem as UnitOption;

    public CostType SelectedCostType =>
        cbTipo.SelectedItem is CostTypeOption costTypeOption
            ? costTypeOption.Value
            : CostType.Abate;

    private void ConfigureScreen()
    {
        Font = new Font("Segoe UI", 9F);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        ViewLoaded?.Invoke(this, EventArgs.Empty);
    }

    public void BindUnits(IEnumerable<UnitOption> units)
    {
        ExecuteOnUiThread(() =>
        {
            cbUnidade.DisplayMember = nameof(UnitOption.DisplayName);
            cbUnidade.ValueMember = nameof(UnitOption.Id);
            cbUnidade.DataSource = units.ToList();
            if (cbUnidade.Items.Count > 0)
            {
                cbUnidade.SelectedIndex = 0;
            }
        });
    }

    public void BindCostTypes(IEnumerable<CostTypeOption> costTypes)
    {
        ExecuteOnUiThread(() =>
        {
            cbTipo.DisplayMember = nameof(CostTypeOption.DisplayName);
            cbTipo.ValueMember = nameof(CostTypeOption.Value);
            cbTipo.DataSource = costTypes.ToList();
            if (cbTipo.Items.Count > 0)
            {
                cbTipo.SelectedIndex = 0;
            }
        });
    }

    public void SetDateRange(DateTime startDate, DateTime endDate)
    {
        ExecuteOnUiThread(() =>
        {
            dtpInicial.Value = startDate;
            dtpFinal.Value = endDate;
        });
    }

    public void SetData(DataTable dataTable)
    {
        ExecuteOnUiThread(() =>
        {
            gridDados.DataSource = dataTable;
        });
    }

    public void SetBusy(bool isBusy, string? statusText = null)
    {
        ExecuteOnUiThread(() =>
        {
            panelFiltros.Enabled = !isBusy;
            btnExportar.Enabled = !isBusy;
            UseWaitCursor = isBusy;

            if (!string.IsNullOrWhiteSpace(statusText))
            {
                lblStatus.Text = statusText;
            }
        });
    }

    public void UpdateStatus(string text)
    {
        ExecuteOnUiThread(() =>
        {
            lblStatus.Text = text;
        });
    }

    public void ShowError(string message, Exception? exception = null)
    {
        ExecuteOnUiThread(() =>
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
        });
    }

    public void ShowWarning(string message, string title = "Validacao")
    {
        ExecuteOnUiThread(() =>
        {
            MessageBox.Show(
                this,
                message,
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        });
    }

    public void ShowInformation(string message, string title = "Informacao")
    {
        ExecuteOnUiThread(() =>
        {
            MessageBox.Show(
                this,
                message,
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        });
    }

    public ExistingRecordDecision ConfirmExistingRecords(DateTime currentDate, int existingCount)
    {
        if (InvokeRequired)
        {
            return (ExistingRecordDecision)Invoke(
                new Func<DateTime, int, ExistingRecordDecision>(ConfirmExistingRecords),
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

    public string? PromptExportFilePath(string suggestedFileName)
    {
        if (InvokeRequired)
        {
            return (string?)Invoke(new Func<string, string?>(PromptExportFilePath), suggestedFileName);
        }

        using var saveFileDialog = new SaveFileDialog
        {
            Filter = "Arquivo CSV (*.csv)|*.csv",
            FileName = suggestedFileName
        };

        return saveFileDialog.ShowDialog(this) == DialogResult.OK
            ? saveFileDialog.FileName
            : null;
    }

    private void btnPesquisar_Click(object? sender, EventArgs e)
    {
        SearchRequested?.Invoke(this, EventArgs.Empty);
    }

    private void btnProcessar_Click(object? sender, EventArgs e)
    {
        ProcessRequested?.Invoke(this, EventArgs.Empty);
    }

    private void btnExportar_Click(object? sender, EventArgs e)
    {
        ExportRequested?.Invoke(this, EventArgs.Empty);
    }

    private void ExecuteOnUiThread(Action action)
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(action);
            return;
        }

        action();
    }
}
