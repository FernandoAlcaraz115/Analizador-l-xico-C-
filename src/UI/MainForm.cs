using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using AnalizadorLexico.Core;
using AnalizadorLexico.Model;

namespace AnalizadorLexico.UI
{
    /// <summary>
    /// Ventana principal de la aplicación gráfica del Analizador Léxico.
    /// Diseñada con estética moderna en modo oscuro.
    /// </summary>
    public class MainForm : Form
    {
        private readonly Lexer _lexer;

        // Controles de interfaz
        private Panel _headerPanel;
        private Panel _toolbarPanel;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _statusLabel;
        private ToolStripStatusLabel _statsLabel;

        private SplitContainer _splitContainer;

        // Editor
        private Panel _editorPanel;
        private Panel _editorHeaderPanel;
        private RichTextBox _txtEditor;
        private RichTextBox _txtLineNumbers;
        private Label _lblEditorInfo;

        // Pestañas de resultados
        private TabControl _tabControl;
        private TabPage _tabTokens;
        private TabPage _tabSymbols;
        private TabPage _tabErrors;
        private TabPage _tabStats;

        private DataGridView _gridTokens;
        private DataGridView _gridSymbols;
        private DataGridView _gridErrors;
        private Panel _pnlStatsContent;

        private ComboBox _cboExamples;

        public MainForm()
        {
            _lexer = new Lexer();
            InitializeComponent();
            ApplyTheme();
            LoadDefaultExample();
        }

        private void InitializeComponent()
        {
            this.Text = "Analizador Léxico - C# | Compiladores y Lenguajes";
            this.Size = new Size(1180, 740);
            this.MinimumSize = new Size(950, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Theme.BgDark;
            this.ForeColor = Theme.TextPrimary;

            // 1. Panel de Encabezado (Header)
            _headerPanel = new Panel();
            _headerPanel.Dock = DockStyle.Top;
            _headerPanel.Height = 65;
            _headerPanel.BackColor = Theme.BgDark;
            _headerPanel.Padding = new Padding(20, 10, 20, 5);

            Label lblTitle = new Label();
            lblTitle.Text = "⚡ ANALIZADOR LÉXICO (SCANNER)";
            lblTitle.Font = Theme.FontTitle;
            lblTitle.ForeColor = Theme.TextPrimary;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 10);

            Label lblSubtitle = new Label();
            lblSubtitle.Text = "Reconocimiento de tokens, tabla de símbolos y validación de errores léxicos";
            lblSubtitle.Font = Theme.FontSubtitle;
            lblSubtitle.ForeColor = Theme.TextSecondary;
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(22, 38);

            _headerPanel.Controls.Add(lblTitle);
            _headerPanel.Controls.Add(lblSubtitle);

            // 2. Barra de Herramientas (Toolbar)
            _toolbarPanel = new Panel();
            _toolbarPanel.Dock = DockStyle.Top;
            _toolbarPanel.Height = 52;
            _toolbarPanel.BackColor = Theme.BgCard;
            _toolbarPanel.Padding = new Padding(15, 8, 15, 8);

            Button btnAnalyze = CreateButton("▶ Analizar", Theme.Primary, Theme.PrimaryHover, 110);
            btnAnalyze.Location = new Point(15, 9);
            btnAnalyze.Click += delegate { AnalyzeCode(); };

            Button btnOpenFile = CreateButton("📂 Abrir", Theme.BgCardHover, Color.FromArgb(71, 85, 105), 90);
            btnOpenFile.Location = new Point(135, 9);
            btnOpenFile.Click += delegate { OpenFile(); };

            Button btnSaveFile = CreateButton("💾 Guardar", Theme.BgCardHover, Color.FromArgb(71, 85, 105), 95);
            btnSaveFile.Location = new Point(235, 9);
            btnSaveFile.Click += delegate { SaveFile(); };

            Button btnClear = CreateButton("🗑 Limpiar", Theme.BgCardHover, Color.FromArgb(71, 85, 105), 90);
            btnClear.Location = new Point(340, 9);
            btnClear.Click += delegate { ClearEditor(); };

            Label lblExample = new Label();
            lblExample.Text = "Ejemplos:";
            lblExample.Font = Theme.FontUi;
            lblExample.ForeColor = Theme.TextSecondary;
            lblExample.AutoSize = true;
            lblExample.Location = new Point(445, 17);

            _cboExamples = new ComboBox();
            _cboExamples.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboExamples.BackColor = Theme.BgInput;
            _cboExamples.ForeColor = Theme.TextPrimary;
            _cboExamples.Font = Theme.FontUi;
            _cboExamples.FlatStyle = FlatStyle.Flat;
            _cboExamples.Width = 230;
            _cboExamples.Location = new Point(515, 13);
            _cboExamples.Items.Add("1. Código Válido (General)");
            _cboExamples.Items.Add("2. Con Errores Léxicos");
            _cboExamples.Items.Add("3. Algoritmo Factorial / Serie");
            _cboExamples.SelectedIndex = 0;
            _cboExamples.SelectedIndexChanged += delegate { LoadSelectedExample(); };

            Button btnExport = CreateButton("📤 Exportar CSV", Theme.BgCardHover, Color.FromArgb(71, 85, 105), 120);
            btnExport.Location = new Point(755, 9);
            btnExport.Click += delegate { ExportTokensToCsv(); };

            _toolbarPanel.Controls.Add(btnAnalyze);
            _toolbarPanel.Controls.Add(btnOpenFile);
            _toolbarPanel.Controls.Add(btnSaveFile);
            _toolbarPanel.Controls.Add(btnClear);
            _toolbarPanel.Controls.Add(lblExample);
            _toolbarPanel.Controls.Add(_cboExamples);
            _toolbarPanel.Controls.Add(btnExport);

            // 3. Barra de Estado (StatusStrip)
            _statusStrip = new StatusStrip();
            _statusStrip.BackColor = Theme.BgCard;
            _statusStrip.ForeColor = Theme.TextSecondary;
            _statusStrip.SizingGrip = false;

            _statusLabel = new ToolStripStatusLabel("Listo. Ingrese código o cargue un archivo y presione 'Analizar'.");
            _statusLabel.Spring = true;
            _statusLabel.TextAlign = ContentAlignment.MiddleLeft;

            _statsLabel = new ToolStripStatusLabel("Tokens: 0 | Errores: 0 | Tiempo: 0 ms");
            _statsLabel.ForeColor = Theme.Primary;

            _statusStrip.Items.Add(_statusLabel);
            _statusStrip.Items.Add(_statsLabel);

            // 4. Split Container Central
            _splitContainer = new SplitContainer();
            _splitContainer.Dock = DockStyle.Fill;
            _splitContainer.BackColor = Theme.BgDark;
            _splitContainer.SplitterDistance = 460;
            _splitContainer.SplitterWidth = 6;

            // Panel Izquierdo: Editor de Código
            _editorPanel = new Panel();
            _editorPanel.Dock = DockStyle.Fill;
            _editorPanel.BackColor = Theme.BgCard;
            _editorPanel.Padding = new Padding(10);

            _editorHeaderPanel = new Panel();
            _editorHeaderPanel.Dock = DockStyle.Top;
            _editorHeaderPanel.Height = 32;

            Label lblEditorTitle = new Label();
            lblEditorTitle.Text = "CÓDIGO FUENTE";
            lblEditorTitle.Font = Theme.FontUiBold;
            lblEditorTitle.ForeColor = Theme.TextPrimary;
            lblEditorTitle.AutoSize = true;
            lblEditorTitle.Location = new Point(4, 5);

            _lblEditorInfo = new Label();
            _lblEditorInfo.Text = "Líneas: 1 | Caracteres: 0";
            _lblEditorInfo.Font = Theme.FontUi;
            _lblEditorInfo.ForeColor = Theme.TextMuted;
            _lblEditorInfo.Dock = DockStyle.Right;
            _lblEditorInfo.TextAlign = ContentAlignment.MiddleRight;

            _editorHeaderPanel.Controls.Add(lblEditorTitle);
            _editorHeaderPanel.Controls.Add(_lblEditorInfo);

            Panel editorBoxContainer = new Panel();
            editorBoxContainer.Dock = DockStyle.Fill;
            editorBoxContainer.BackColor = Theme.BgInput;
            editorBoxContainer.BorderStyle = BorderStyle.FixedSingle;

            _txtLineNumbers = new RichTextBox();
            _txtLineNumbers.Dock = DockStyle.Left;
            _txtLineNumbers.Width = 42;
            _txtLineNumbers.BackColor = Color.FromArgb(20, 27, 45);
            _txtLineNumbers.ForeColor = Theme.TextMuted;
            _txtLineNumbers.BorderStyle = BorderStyle.None;
            _txtLineNumbers.ScrollBars = RichTextBoxScrollBars.None;
            _txtLineNumbers.ReadOnly = true;
            _txtLineNumbers.Font = Theme.FontCode;
            _txtLineNumbers.SelectionAlignment = HorizontalAlignment.Right;
            _txtLineNumbers.Cursor = Cursors.Default;

            _txtEditor = new RichTextBox();
            _txtEditor.Dock = DockStyle.Fill;
            _txtEditor.BackColor = Theme.BgInput;
            _txtEditor.ForeColor = Theme.TextPrimary;
            _txtEditor.BorderStyle = BorderStyle.None;
            _txtEditor.Font = Theme.FontCode;
            _txtEditor.AcceptsTab = true;
            _txtEditor.WordWrap = false;
            _txtEditor.ScrollBars = RichTextBoxScrollBars.Both;

            _txtEditor.TextChanged += delegate { UpdateEditorMetrics(); };
            _txtEditor.VScroll += delegate { SyncLineNumbers(); };

            editorBoxContainer.Controls.Add(_txtEditor);
            editorBoxContainer.Controls.Add(_txtLineNumbers);

            _editorPanel.Controls.Add(editorBoxContainer);
            _editorPanel.Controls.Add(_editorHeaderPanel);

            _splitContainer.Panel1.Controls.Add(_editorPanel);

            // Panel Derecho: TabControl de Resultados
            _tabControl = new TabControl();
            _tabControl.Dock = DockStyle.Fill;
            _tabControl.Font = Theme.FontUi;
            _tabControl.Padding = new Point(14, 6);

            // Tab 1: Tokens
            _tabTokens = new TabPage("Tokens (0)");
            _tabTokens.BackColor = Theme.BgCard;
            _gridTokens = CreateStyledGrid();
            SetupTokensGrid();
            _tabTokens.Controls.Add(_gridTokens);

            // Tab 2: Tabla de Símbolos
            _tabSymbols = new TabPage("Tabla de Símbolos (0)");
            _tabSymbols.BackColor = Theme.BgCard;
            _gridSymbols = CreateStyledGrid();
            SetupSymbolsGrid();
            _tabSymbols.Controls.Add(_gridSymbols);

            // Tab 3: Errores Léxicos
            _tabErrors = new TabPage("Errores Léxicos (0)");
            _tabErrors.BackColor = Theme.BgCard;
            _gridErrors = CreateStyledGrid();
            SetupErrorsGrid();
            _tabErrors.Controls.Add(_gridErrors);

            // Tab 4: Estadísticas
            _tabStats = new TabPage("Métricas & Resumen");
            _tabStats.BackColor = Theme.BgCard;
            _pnlStatsContent = new Panel();
            _pnlStatsContent.Dock = DockStyle.Fill;
            _pnlStatsContent.AutoScroll = true;
            _pnlStatsContent.Padding = new Padding(20);
            _tabStats.Controls.Add(_pnlStatsContent);

            _tabControl.TabPages.Add(_tabTokens);
            _tabControl.TabPages.Add(_tabSymbols);
            _tabControl.TabPages.Add(_tabErrors);
            _tabControl.TabPages.Add(_tabStats);

            _splitContainer.Panel2.Controls.Add(_tabControl);

            // Ensamblado Principal
            this.Controls.Add(_splitContainer);
            this.Controls.Add(_toolbarPanel);
            this.Controls.Add(_headerPanel);
            this.Controls.Add(_statusStrip);
        }

        private Button CreateButton(string text, Color backColor, Color hoverColor, int width)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Width = width;
            btn.Height = 34;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = backColor;
            btn.ForeColor = Theme.TextPrimary;
            btn.Font = Theme.FontUiBold;
            btn.Cursor = Cursors.Hand;

            btn.MouseEnter += delegate { btn.BackColor = hoverColor; };
            btn.MouseLeave += delegate { btn.BackColor = backColor; };

            return btn;
        }

        private DataGridView CreateStyledGrid()
        {
            DataGridView grid = new DataGridView();
            grid.Dock = DockStyle.Fill;
            grid.BackgroundColor = Theme.BgCard;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = Color.FromArgb(45, 55, 72);
            grid.RowHeadersVisible = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = true;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight = 34;

            // Estilo de encabezados
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 27, 45);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Theme.TextPrimary;
            grid.ColumnHeadersDefaultCellStyle.Font = Theme.FontUiBold;
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            // Estilo de filas
            grid.DefaultCellStyle.BackColor = Theme.BgCard;
            grid.DefaultCellStyle.ForeColor = Theme.TextPrimary;
            grid.DefaultCellStyle.SelectionBackColor = Theme.BgCardHover;
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            grid.DefaultCellStyle.Font = Theme.FontUi;
            grid.RowTemplate.Height = 28;

            return grid;
        }

        private void SetupTokensGrid()
        {
            _gridTokens.Columns.Clear();
            _gridTokens.Columns.Add("Index", "N°");
            _gridTokens.Columns.Add("Line", "Línea");
            _gridTokens.Columns.Add("Col", "Columna");
            _gridTokens.Columns.Add("Lexeme", "Lexema");
            _gridTokens.Columns.Add("Type", "Tipo de Token");
            _gridTokens.Columns.Add("Category", "Categoría");
            _gridTokens.Columns.Add("Description", "Descripción");

            _gridTokens.Columns["Index"].FillWeight = 35;
            _gridTokens.Columns["Line"].FillWeight = 40;
            _gridTokens.Columns["Col"].FillWeight = 45;
            _gridTokens.Columns["Lexeme"].FillWeight = 85;
            _gridTokens.Columns["Type"].FillWeight = 85;
            _gridTokens.Columns["Category"].FillWeight = 85;
            _gridTokens.Columns["Description"].FillWeight = 130;

            _gridTokens.Columns["Index"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _gridTokens.Columns["Line"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _gridTokens.Columns["Col"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _gridTokens.Columns["Lexeme"].DefaultCellStyle.Font = Theme.FontCode;
        }

        private void SetupSymbolsGrid()
        {
            _gridSymbols.Columns.Clear();
            _gridSymbols.Columns.Add("Id", "ID");
            _gridSymbols.Columns.Add("Name", "Identificador");
            _gridSymbols.Columns.Add("Type", "Tipo Inferido");
            _gridSymbols.Columns.Add("Count", "Apariciones");
            _gridSymbols.Columns.Add("FirstPos", "Primera Posición");
            _gridSymbols.Columns.Add("Positions", "Todas las Posiciones (L:C)");

            _gridSymbols.Columns["Id"].FillWeight = 35;
            _gridSymbols.Columns["Name"].FillWeight = 90;
            _gridSymbols.Columns["Type"].FillWeight = 70;
            _gridSymbols.Columns["Count"].FillWeight = 55;
            _gridSymbols.Columns["FirstPos"].FillWeight = 70;
            _gridSymbols.Columns["Positions"].FillWeight = 160;

            _gridSymbols.Columns["Id"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _gridSymbols.Columns["Count"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _gridSymbols.Columns["FirstPos"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _gridSymbols.Columns["Name"].DefaultCellStyle.Font = Theme.FontCode;
        }

        private void SetupErrorsGrid()
        {
            _gridErrors.Columns.Clear();
            _gridErrors.Columns.Add("Index", "N°");
            _gridErrors.Columns.Add("Line", "Línea");
            _gridErrors.Columns.Add("Col", "Columna");
            _gridErrors.Columns.Add("Lexeme", "Carácter / Lexema");
            _gridErrors.Columns.Add("Message", "Descripción del Error");
            _gridErrors.Columns.Add("Suggestion", "Sugerencia");

            _gridErrors.Columns["Index"].FillWeight = 35;
            _gridErrors.Columns["Line"].FillWeight = 40;
            _gridErrors.Columns["Col"].FillWeight = 45;
            _gridErrors.Columns["Lexeme"].FillWeight = 70;
            _gridErrors.Columns["Message"].FillWeight = 140;
            _gridErrors.Columns["Suggestion"].FillWeight = 140;

            _gridErrors.Columns["Index"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _gridErrors.Columns["Line"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _gridErrors.Columns["Col"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _gridErrors.Columns["Lexeme"].DefaultCellStyle.Font = Theme.FontCode;
            _gridErrors.Columns["Lexeme"].DefaultCellStyle.ForeColor = Theme.Danger;
        }

        private void ApplyTheme()
        {
            // Personalización adicional de pestañas si el sistema operativo lo permite
        }

        private void SyncLineNumbers()
        {
            Point pt = new Point(0, 0);
            int firstIndex = _txtEditor.GetCharIndexFromPosition(pt);
            int firstLine = _txtEditor.GetLineFromCharIndex(firstIndex);

            pt.X = _txtEditor.ClientRectangle.Width;
            pt.Y = _txtEditor.ClientRectangle.Height;
            int lastIndex = _txtEditor.GetCharIndexFromPosition(pt);
            int lastLine = _txtEditor.GetLineFromCharIndex(lastIndex);

            StringBuilder sb = new StringBuilder();
            for (int i = firstLine; i <= lastLine + 1; i++)
            {
                sb.AppendLine((i + 1).ToString());
            }

            _txtLineNumbers.Text = sb.ToString();
        }

        private void UpdateEditorMetrics()
        {
            int lineCount = _txtEditor.Lines.Length;
            int charCount = _txtEditor.TextLength;
            _lblEditorInfo.Text = string.Format("Líneas: {0} | Caracteres: {1}", lineCount, charCount);
            SyncLineNumbers();
        }

        private void AnalyzeCode()
        {
            string source = _txtEditor.Text;

            _statusLabel.Text = "Analizando código...";
            _statusStrip.Update();

            _lexer.Tokenize(source);

            // Llenar tabla de Tokens
            _gridTokens.Rows.Clear();
            foreach (Token tok in _lexer.Tokens)
            {
                int rowIndex = _gridTokens.Rows.Add(
                    tok.Index,
                    tok.Line,
                    tok.Column,
                    tok.Lexeme,
                    tok.Type.ToString(),
                    tok.Category.ToString(),
                    tok.Description
                );

                // Colorear categoría
                Color catColor = Theme.GetCategoryColor(tok.Category);
                _gridTokens.Rows[rowIndex].Cells["Category"].Style.ForeColor = catColor;
                _gridTokens.Rows[rowIndex].Cells["Category"].Style.Font = Theme.FontUiBold;
            }

            // Llenar tabla de Símbolos
            _gridSymbols.Rows.Clear();
            List<SymbolItem> symbols = _lexer.Symbols.GetAllSymbols();
            foreach (SymbolItem sym in symbols)
            {
                _gridSymbols.Rows.Add(
                    sym.Id,
                    sym.Name,
                    sym.InferredType,
                    sym.OccurrencesCount,
                    string.Format("L:{0}, C:{1}", sym.FirstLine, sym.FirstColumn),
                    sym.PositionsSummary
                );
            }

            // Llenar tabla de Errores
            _gridErrors.Rows.Clear();
            foreach (LexicalError err in _lexer.Errors)
            {
                int rowIndex = _gridErrors.Rows.Add(
                    err.Index,
                    err.Line,
                    err.Column,
                    err.InvalidLexeme,
                    err.Message,
                    err.Suggestion
                );

                _gridErrors.Rows[rowIndex].Cells["Message"].Style.ForeColor = Theme.Danger;
            }

            // Actualizar etiquetas de pestañas
            _tabTokens.Text = string.Format("Tokens ({0})", _lexer.Tokens.Count);
            _tabSymbols.Text = string.Format("Tabla de Símbolos ({0})", symbols.Count);
            _tabErrors.Text = string.Format("Errores Léxicos ({0})", _lexer.Errors.Count);

            // Generar vista de Estadísticas
            RenderStats();

            // Mensajes de barra de estado
            if (_lexer.Errors.Count > 0)
            {
                _statusLabel.Text = string.Format("⚠ Análisis finalizado con {0} error(es) léxico(s).", _lexer.Errors.Count);
                _statusLabel.ForeColor = Theme.Danger;
                _tabControl.SelectedTab = _tabErrors;
            }
            else
            {
                _statusLabel.Text = "✔ Análisis completado exitosamente sin errores léxicos.";
                _statusLabel.ForeColor = Theme.Success;
            }

            _statsLabel.Text = string.Format("Tokens: {0} | Errores: {1} | Tiempo: {2:0.00} ms",
                _lexer.Stats.TotalTokens, _lexer.Stats.TotalErrors, _lexer.Stats.ElapsedMilliseconds);
        }

        private void RenderStats()
        {
            _pnlStatsContent.Controls.Clear();

            int y = 10;

            Label lblResumenTitle = new Label();
            lblResumenTitle.Text = "RESUMEN DEL ANÁLISIS LÉXICO";
            lblResumenTitle.Font = Theme.FontTitle;
            lblResumenTitle.ForeColor = Theme.TextPrimary;
            lblResumenTitle.AutoSize = true;
            lblResumenTitle.Location = new Point(10, y);
            _pnlStatsContent.Controls.Add(lblResumenTitle);

            y += 40;

            // Tarjetas de Métricas Generales
            AddMetricCard(_pnlStatsContent, 10, y, 160, 80, "TOTAL TOKENS", _lexer.Stats.TotalTokens.ToString(), Theme.Primary);
            AddMetricCard(_pnlStatsContent, 180, y, 160, 80, "ERRORES LÉXICOS", _lexer.Stats.TotalErrors.ToString(), _lexer.Stats.TotalErrors > 0 ? Theme.Danger : Theme.Success);
            AddMetricCard(_pnlStatsContent, 350, y, 160, 80, "SÍMBOLOS ÚNICOS", _lexer.Stats.UniqueSymbols.ToString(), Theme.Purple);
            AddMetricCard(_pnlStatsContent, 520, y, 160, 80, "TIEMPO ANÁLISIS", string.Format("{0:0.00} ms", _lexer.Stats.ElapsedMilliseconds), Theme.Warning);

            y += 100;

            Label lblBreakdown = new Label();
            lblBreakdown.Text = "DISTRIBUCIÓN POR CATEGORÍAS DE TOKEN";
            lblBreakdown.Font = Theme.FontUiBold;
            lblBreakdown.ForeColor = Theme.TextSecondary;
            lblBreakdown.AutoSize = true;
            lblBreakdown.Location = new Point(10, y);
            _pnlStatsContent.Controls.Add(lblBreakdown);

            y += 30;

            foreach (KeyValuePair<TokenCategory, int> pair in _lexer.Stats.CategoryCounts)
            {
                Panel row = new Panel();
                row.Size = new Size(500, 28);
                row.Location = new Point(10, y);

                Label lblCat = new Label();
                lblCat.Text = pair.Key.ToString();
                lblCat.Font = Theme.FontUi;
                lblCat.ForeColor = Theme.GetCategoryColor(pair.Key);
                lblCat.AutoSize = true;
                lblCat.Location = new Point(5, 5);

                Label lblVal = new Label();
                lblVal.Text = pair.Value.ToString();
                lblVal.Font = Theme.FontUiBold;
                lblVal.ForeColor = Theme.TextPrimary;
                lblVal.Dock = DockStyle.Right;
                lblVal.TextAlign = ContentAlignment.MiddleRight;

                row.Controls.Add(lblCat);
                row.Controls.Add(lblVal);
                _pnlStatsContent.Controls.Add(row);

                y += 30;
            }
        }

        private void AddMetricCard(Panel parent, int x, int y, int width, int height, string title, string value, Color accentColor)
        {
            Panel card = new Panel();
            card.Size = new Size(width, height);
            card.Location = new Point(x, y);
            card.BackColor = Color.FromArgb(20, 27, 45);
            card.BorderStyle = BorderStyle.FixedSingle;

            Label lblVal = new Label();
            lblVal.Text = value;
            lblVal.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
            lblVal.ForeColor = accentColor;
            lblVal.AutoSize = true;
            lblVal.Location = new Point(12, 10);

            Label lblT = new Label();
            lblT.Text = title;
            lblT.Font = Theme.FontBadge;
            lblT.ForeColor = Theme.TextSecondary;
            lblT.AutoSize = true;
            lblT.Location = new Point(14, 48);

            card.Controls.Add(lblVal);
            card.Controls.Add(lblT);
            parent.Controls.Add(card);
        }

        private void OpenFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Archivos de Código (*.txt;*.cs;*.c;*.cpp;*.java)|*.txt;*.cs;*.c;*.cpp;*.java|Todos los archivos (*.*)|*.*";
            ofd.Title = "Abrir código fuente para analizar";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string text = File.ReadAllText(ofd.FileName, Encoding.UTF8);
                    _txtEditor.Text = text;
                    _statusLabel.Text = "Archivo cargado: " + Path.GetFileName(ofd.FileName);
                    AnalyzeCode();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al leer el archivo:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SaveFile()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Archivos de Texto (*.txt)|*.txt|Archivos de Código (*.cs)|*.cs|Todos los archivos (*.*)|*.*";
            sfd.Title = "Guardar código fuente actual";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(sfd.FileName, _txtEditor.Text, Encoding.UTF8);
                    _statusLabel.Text = "Archivo guardado exitosamente en " + Path.GetFileName(sfd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al guardar el archivo:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearEditor()
        {
            _txtEditor.Clear();
            _gridTokens.Rows.Clear();
            _gridSymbols.Rows.Clear();
            _gridErrors.Rows.Clear();
            _pnlStatsContent.Controls.Clear();

            _tabTokens.Text = "Tokens (0)";
            _tabSymbols.Text = "Tabla de Símbolos (0)";
            _tabErrors.Text = "Errores Léxicos (0)";

            _statusLabel.Text = "Editor limpiado.";
            _statusLabel.ForeColor = Theme.TextSecondary;
            _statsLabel.Text = "Tokens: 0 | Errores: 0 | Tiempo: 0 ms";
        }

        private void ExportTokensToCsv()
        {
            if (_lexer.Tokens == null || _lexer.Tokens.Count == 0)
            {
                MessageBox.Show("Primero ejecute el análisis del código para generar los tokens.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Archivo CSV (*.csv)|*.csv";
            sfd.Title = "Exportar lista de tokens a CSV";
            sfd.FileName = "tokens_analizados.csv";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Indice,Linea,Columna,Lexema,TipoToken,Categoria,Descripcion");

                    foreach (Token t in _lexer.Tokens)
                    {
                        string safeLexeme = t.Lexeme.Replace("\"", "\"\"");
                        string safeDesc = t.Description.Replace("\"", "\"\"");
                        sb.AppendLine(string.Format("{0},{1},{2},\"{3}\",{4},{5},\"{6}\"",
                            t.Index, t.Line, t.Column, safeLexeme, t.Type, t.Category, safeDesc));
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Tokens exportados correctamente a:\n" + sfd.FileName, "Exportación exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al exportar archivo CSV:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadDefaultExample()
        {
            _cboExamples.SelectedIndex = 0;
            LoadSelectedExample();
        }

        private void LoadSelectedExample()
        {
            switch (_cboExamples.SelectedIndex)
            {
                case 0:
                    _txtEditor.Text = GetGeneralExampleCode();
                    break;
                case 1:
                    _txtEditor.Text = GetErrorExampleCode();
                    break;
                case 2:
                    _txtEditor.Text = GetAlgorithmExampleCode();
                    break;
            }
            AnalyzeCode();
        }

        private string GetGeneralExampleCode()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("// ==========================================");
            sb.AppendLine("// Programa de prueba léxica en C#");
            sb.AppendLine("// ==========================================");
            sb.AppendLine("class Calculadora");
            sb.AppendLine("{");
            sb.AppendLine("    int totalOperaciones = 0;");
            sb.AppendLine("    float pi = 3.14159;");
            sb.AppendLine("    string mensaje = \"Iniciando calculos...\";");
            sb.AppendLine("    bool activo = true;");
            sb.AppendLine("");
            sb.AppendLine("    /* Funcion para sumar dos valores enteros */");
            sb.AppendLine("    int sumar(int a, int b)");
            sb.AppendLine("    {");
            sb.AppendLine("        int resultado = a + b;");
            sb.AppendLine("        totalOperaciones++;");
            sb.AppendLine("        return resultado;");
            sb.AppendLine("    }");
            sb.AppendLine("");
            sb.AppendLine("    void main()");
            sb.AppendLine("    {");
            sb.AppendLine("        println(mensaje);");
            sb.AppendLine("        int x = 15;");
            sb.AppendLine("        int y = 25;");
            sb.AppendLine("        if (x <= y && activo == true)");
            sb.AppendLine("        {");
            sb.AppendLine("            int r = sumar(x, y);");
            sb.AppendLine("            print(\"Resultado suma: \");");
            sb.AppendLine("            println(r);");
            sb.AppendLine("        }");
            sb.AppendLine("        else");
            sb.AppendLine("        {");
            sb.AppendLine("            println(\"Condicion no cumplida\");");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private string GetErrorExampleCode()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("// Programa con errores lexicos deliberados para pruebas");
            sb.AppendLine("int a = 100;");
            sb.AppendLine("float malNumero = 3.14.15.9; // Error: numero con multiples puntos");
            sb.AppendLine("int ident_invalido = 99variable; // Error: numero seguido de letras");
            sb.AppendLine("string texto = \"Cadena sin cerrar antes del fin de linea");
            sb.AppendLine("char c = 'demasiados_caracteres'; // Error: char no valido");
            sb.AppendLine("int simbolo_raro = a @ 5 # 3 $ 10; // Error: caracteres ilegales @, #, $");
            sb.AppendLine("bool logico = (a & 10) | (a > 5); // Error: operadores logicos incompletos");
            sb.AppendLine("/* Comentario de bloque que nunca se cerro antes de EOF");
            return sb.ToString();
        }

        private string GetAlgorithmExampleCode()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("// Algoritmo de calculo de Factorial y Serie Fibonacci");
            sb.AppendLine("function int factorial(int n)");
            sb.AppendLine("{");
            sb.AppendLine("    if (n <= 1)");
            sb.AppendLine("    {");
            sb.AppendLine("        return 1;");
            sb.AppendLine("    }");
            sb.AppendLine("    return n * factorial(n - 1);");
            sb.AppendLine("}");
            sb.AppendLine("");
            sb.AppendLine("function void fibonacci(int limite)");
            sb.AppendLine("{");
            sb.AppendLine("    int a = 0;");
            sb.AppendLine("    int b = 1;");
            sb.AppendLine("    int temp = 0;");
            sb.AppendLine("    int i = 0;");
            sb.AppendLine("");
            sb.AppendLine("    println(\"Serie Fibonacci:\");");
            sb.AppendLine("    while (i < limite)");
            sb.AppendLine("    {");
            sb.AppendLine("        println(a);");
            sb.AppendLine("        temp = a + b;");
            sb.AppendLine("        a = b;");
            sb.AppendLine("        b = temp;");
            sb.AppendLine("        i++;");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
