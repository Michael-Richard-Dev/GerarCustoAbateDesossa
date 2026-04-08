#nullable disable

namespace GerarCustoAbateDesossa.Desktop;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private Panel panelFiltros = null!;
    private Label lblDataInicial = null!;
    private Label lblDataFinal = null!;
    private Label lblUnidade = null!;
    private Label lblTipo = null!;
    private DateTimePicker dtpInicial = null!;
    private DateTimePicker dtpFinal = null!;
    private ComboBox cbUnidade = null!;
    private ComboBox cbTipo = null!;
    private Button btnPesquisar = null!;
    private Button btnProcessar = null!;
    private Button btnExportar = null!;
    private DataGridView gridDados = null!;
    private StatusStrip statusStrip = null!;
    private ToolStripStatusLabel lblStatus = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components is not null)
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        panelFiltros = new Panel();
        lblDataInicial = new Label();
        lblDataFinal = new Label();
        lblUnidade = new Label();
        lblTipo = new Label();
        dtpInicial = new DateTimePicker();
        dtpFinal = new DateTimePicker();
        cbUnidade = new ComboBox();
        cbTipo = new ComboBox();
        btnPesquisar = new Button();
        btnProcessar = new Button();
        btnExportar = new Button();
        gridDados = new DataGridView();
        statusStrip = new StatusStrip();
        lblStatus = new ToolStripStatusLabel();
        panelFiltros.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridDados).BeginInit();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // panelFiltros
        // 
        panelFiltros.Controls.Add(btnExportar);
        panelFiltros.Controls.Add(btnProcessar);
        panelFiltros.Controls.Add(btnPesquisar);
        panelFiltros.Controls.Add(cbTipo);
        panelFiltros.Controls.Add(cbUnidade);
        panelFiltros.Controls.Add(dtpFinal);
        panelFiltros.Controls.Add(dtpInicial);
        panelFiltros.Controls.Add(lblTipo);
        panelFiltros.Controls.Add(lblUnidade);
        panelFiltros.Controls.Add(lblDataFinal);
        panelFiltros.Controls.Add(lblDataInicial);
        panelFiltros.Dock = DockStyle.Top;
        panelFiltros.Location = new Point(0, 0);
        panelFiltros.Name = "panelFiltros";
        panelFiltros.Size = new Size(980, 92);
        panelFiltros.TabIndex = 0;
        // 
        // lblDataInicial
        // 
        lblDataInicial.AutoSize = true;
        lblDataInicial.Location = new Point(16, 15);
        lblDataInicial.Name = "lblDataInicial";
        lblDataInicial.Size = new Size(64, 15);
        lblDataInicial.TabIndex = 0;
        lblDataInicial.Text = "Data inicial";
        // 
        // lblDataFinal
        // 
        lblDataFinal.AutoSize = true;
        lblDataFinal.Location = new Point(127, 15);
        lblDataFinal.Name = "lblDataFinal";
        lblDataFinal.Size = new Size(58, 15);
        lblDataFinal.TabIndex = 1;
        lblDataFinal.Text = "Data final";
        // 
        // lblUnidade
        // 
        lblUnidade.AutoSize = true;
        lblUnidade.Location = new Point(238, 15);
        lblUnidade.Name = "lblUnidade";
        lblUnidade.Size = new Size(53, 15);
        lblUnidade.TabIndex = 2;
        lblUnidade.Text = "Unidade";
        // 
        // lblTipo
        // 
        lblTipo.AutoSize = true;
        lblTipo.Location = new Point(570, 15);
        lblTipo.Name = "lblTipo";
        lblTipo.Size = new Size(77, 15);
        lblTipo.TabIndex = 3;
        lblTipo.Text = "Tipo de custo";
        // 
        // dtpInicial
        // 
        dtpInicial.Format = DateTimePickerFormat.Short;
        dtpInicial.Location = new Point(16, 38);
        dtpInicial.Name = "dtpInicial";
        dtpInicial.Size = new Size(95, 23);
        dtpInicial.TabIndex = 0;
        // 
        // dtpFinal
        // 
        dtpFinal.Format = DateTimePickerFormat.Short;
        dtpFinal.Location = new Point(127, 38);
        dtpFinal.Name = "dtpFinal";
        dtpFinal.Size = new Size(95, 23);
        dtpFinal.TabIndex = 1;
        // 
        // cbUnidade
        // 
        cbUnidade.DropDownStyle = ComboBoxStyle.DropDownList;
        cbUnidade.FormattingEnabled = true;
        cbUnidade.Location = new Point(238, 38);
        cbUnidade.Name = "cbUnidade";
        cbUnidade.Size = new Size(316, 23);
        cbUnidade.TabIndex = 2;
        // 
        // cbTipo
        // 
        cbTipo.DropDownStyle = ComboBoxStyle.DropDownList;
        cbTipo.FormattingEnabled = true;
        cbTipo.Location = new Point(570, 38);
        cbTipo.Name = "cbTipo";
        cbTipo.Size = new Size(150, 23);
        cbTipo.TabIndex = 3;
        // 
        // btnPesquisar
        // 
        btnPesquisar.Location = new Point(738, 37);
        btnPesquisar.Name = "btnPesquisar";
        btnPesquisar.Size = new Size(75, 25);
        btnPesquisar.TabIndex = 4;
        btnPesquisar.Text = "Pesquisar";
        btnPesquisar.UseVisualStyleBackColor = true;
        btnPesquisar.Click += btnPesquisar_Click;
        // 
        // btnProcessar
        // 
        btnProcessar.Location = new Point(819, 37);
        btnProcessar.Name = "btnProcessar";
        btnProcessar.Size = new Size(75, 25);
        btnProcessar.TabIndex = 5;
        btnProcessar.Text = "Processar";
        btnProcessar.UseVisualStyleBackColor = true;
        btnProcessar.Click += btnProcessar_Click;
        // 
        // btnExportar
        // 
        btnExportar.Location = new Point(900, 37);
        btnExportar.Name = "btnExportar";
        btnExportar.Size = new Size(68, 25);
        btnExportar.TabIndex = 6;
        btnExportar.Text = "Exportar";
        btnExportar.UseVisualStyleBackColor = true;
        btnExportar.Click += btnExportar_Click;
        // 
        // gridDados
        // 
        gridDados.AllowUserToAddRows = false;
        gridDados.AllowUserToDeleteRows = false;
        gridDados.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        gridDados.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        gridDados.Dock = DockStyle.Fill;
        gridDados.Location = new Point(0, 92);
        gridDados.Name = "gridDados";
        gridDados.ReadOnly = true;
        gridDados.RowHeadersVisible = false;
        gridDados.Size = new Size(980, 457);
        gridDados.TabIndex = 1;
        // 
        // statusStrip
        // 
        statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus });
        statusStrip.Location = new Point(0, 549);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(980, 22);
        statusStrip.TabIndex = 2;
        // 
        // lblStatus
        // 
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(39, 17);
        lblStatus.Text = "Pronto";
        // 
        // MainForm
        // 
        AcceptButton = btnPesquisar;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(980, 571);
        Controls.Add(gridDados);
        Controls.Add(statusStrip);
        Controls.Add(panelFiltros);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Gerar Custo Abate e Desossa";
        panelFiltros.ResumeLayout(false);
        panelFiltros.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)gridDados).EndInit();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
