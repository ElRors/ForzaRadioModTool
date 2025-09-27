using ForzaRadioModTool.Configuration;
using ForzaRadioModTool.Core.Interfaces;
using ForzaRadioModTool.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ForzaRadioModTool.UI.Forms
{
    public partial class MainForm : Form
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MainForm> _logger;
        private readonly IRadioManager _radioManager;
        private readonly IFileManager _fileManager;
        private readonly IAudioProcessor _audioProcessor;
        private readonly AppSettings _settings;

        private string? _currentGamePath;
        private string _currentLanguage = "EN";
        private List<RadioInfo> _currentRadios = new();
        private List<SongInfo> _currentSongs = new();
        private bool _isInitializing = false;

        // UI Controls
        private TextBox txtGamePath = null!;
        private Button btnBrowseGamePath = null!;
        private ComboBox cmbLanguages = null!;
        private ListBox lstRadios = null!;
        private DataGridView dgvSongs = null!;
        private Button btnApplyChanges = null!;
        private Button btnExtractBank = null!;
        private Button btnInsertIntoGame = null!;
        private Button btnExit = null!;
        private ProgressBar progressBar = null!;
        private Label lblStatus = null!;
        private Label lblGamePath = null!;
        private Label lblLanguage = null!;
        private Label lblRadios = null!;
        private Label lblSongs = null!;

        public MainForm(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<MainForm>>();
            _radioManager = serviceProvider.GetRequiredService<IRadioManager>();
            _fileManager = serviceProvider.GetRequiredService<IFileManager>();
            _audioProcessor = serviceProvider.GetRequiredService<IAudioProcessor>();
            _settings = serviceProvider.GetRequiredService<AppSettings>();

            _logger.LogInformation("Initializing MainForm");
            
            InitializeComponent();
            LoadConfiguration();
        }

        private void InitializeComponent()
        {
            _isInitializing = true;

            SuspendLayout();
            
            // Form properties
            Text = Strings.Get(_currentLanguage, "AppTitle");
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(_settings.UI.MinWindowWidth, _settings.UI.MinWindowHeight);
            MinimumSize = new Size(_settings.UI.MinWindowWidth, _settings.UI.MinWindowHeight);
            
            // Cargar icono personalizado
            LoadCustomIcon();

            CreateControls();
            LayoutControls();
            SetupEventHandlers();
            ApplyLanguage();

            ResumeLayout(false);
            PerformLayout();

            _isInitializing = false;
            _logger.LogInformation("MainForm initialization completed");
        }

        private void CreateControls()
        {
            // Game path section
            lblGamePath = new Label
            {
                Text = Strings.Get(_currentLanguage, "DirectorioJuego"),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true
            };

            txtGamePath = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                ReadOnly = true,
                BackColor = SystemColors.Control
            };

            btnBrowseGamePath = new Button
            {
                Text = Strings.Get(_currentLanguage, "BuscarCarpeta"),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(120, 30)
            };

            // Language section
            lblLanguage = new Label
            {
                Text = Strings.Get(_currentLanguage, "Idioma"),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true
            };

            cmbLanguages = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Size = new Size(100, 25),
                Enabled = false
            };
            cmbLanguages.Items.AddRange(_settings.UI.SupportedLanguages);
            cmbLanguages.SelectedItem = _currentLanguage;

            // Radios section
            lblRadios = new Label
            {
                Text = Strings.Get(_currentLanguage, "ListaRadios"),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true
            };

            lstRadios = new ListBox
            {
                Font = new Font("Segoe UI", 10),
                Size = new Size(250, 300),
                Enabled = false
            };

            // Songs section
            lblSongs = new Label
            {
                Text = Strings.Get(_currentLanguage, "CancionesRadio"),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true
            };

            dgvSongs = new DataGridView
            {
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = false,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9),
                Enabled = false
            };

            // Add columns
            dgvSongs.Columns.Add(new DataGridViewButtonColumn
            {
                HeaderText = "▶",
                Text = Strings.Get(_currentLanguage, "Play"),
                UseColumnTextForButtonValue = true,
                Width = 60,
                Name = "PlayColumn"
            });

            dgvSongs.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = Strings.Get(_currentLanguage, "ColID"),
                ReadOnly = true,
                Width = 80,
                Name = "IdColumn"
            });

            dgvSongs.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = Strings.Get(_currentLanguage, "ColNombre"),
                ReadOnly = false,
                Name = "NameColumn"
            });

            dgvSongs.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = Strings.Get(_currentLanguage, "ColArtista"),
                ReadOnly = false,
                Name = "ArtistColumn"
            });

            dgvSongs.Columns.Add(new DataGridViewButtonColumn
            {
                HeaderText = Strings.Get(_currentLanguage, "ReemplazarCancion"),
                Text = Strings.Get(_currentLanguage, "ReemplazarCancion"),
                UseColumnTextForButtonValue = true,
                Name = "ReplaceColumn"
            });

            // Action buttons
            btnApplyChanges = new Button
            {
                Text = Strings.Get(_currentLanguage, "AplicarCambios"),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(150, 35),
                Enabled = false
            };

            btnExtractBank = new Button
            {
                Text = Strings.Get(_currentLanguage, "RunFMOD"),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(150, 35),
                Enabled = false
            };

            btnInsertIntoGame = new Button
            {
                Text = Strings.Get(_currentLanguage, "InsertarEnJuego"),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(180, 35),
                Enabled = false
            };

            btnExit = new Button
            {
                Text = Strings.Get(_currentLanguage, "Salir"),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(100, 35)
            };

            // Status controls
            progressBar = new ProgressBar
            {
                Style = ProgressBarStyle.Continuous,
                Visible = false
            };

            lblStatus = new Label
            {
                Text = "Ready",
                Font = new Font("Segoe UI", 9),
                AutoSize = true,
                ForeColor = SystemColors.ControlDarkDark
            };

            // Add controls to form
            Controls.AddRange(new Control[]
            {
                lblGamePath, txtGamePath, btnBrowseGamePath,
                lblLanguage, cmbLanguages,
                lblRadios, lstRadios,
                lblSongs, dgvSongs,
                btnApplyChanges, btnExtractBank, btnInsertIntoGame, btnExit,
                progressBar, lblStatus
            });

            // Setup tooltips if enabled
            if (_settings.UI.ShowTooltips)
            {
                var toolTip = new ToolTip();
                toolTip.SetToolTip(txtGamePath, "Select the main game directory (where the game executable is located)");
                toolTip.SetToolTip(cmbLanguages, "Choose the language for radio information");
                toolTip.SetToolTip(lstRadios, "Select a radio station to edit its songs");
                toolTip.SetToolTip(btnApplyChanges, "Save metadata changes to XML files");
                toolTip.SetToolTip(btnExtractBank, "Extract audio files from bank files using FMOD tools");
                toolTip.SetToolTip(btnInsertIntoGame, "Copy modified bank files back to the game directory");
            }
        }

        private void LayoutControls()
        {
            const int margin = 20;
            const int spacing = 10;
            int currentY = margin;

            // Game path row
            lblGamePath.Location = new Point(margin, currentY);
            currentY += lblGamePath.Height + 5;

            txtGamePath.Location = new Point(margin, currentY);
            txtGamePath.Width = ClientSize.Width - btnBrowseGamePath.Width - cmbLanguages.Width - lblLanguage.Width - margin * 2 - spacing * 3;

            btnBrowseGamePath.Location = new Point(txtGamePath.Right + spacing, currentY - 3);
            
            lblLanguage.Location = new Point(btnBrowseGamePath.Right + spacing, lblGamePath.Top);
            cmbLanguages.Location = new Point(lblLanguage.Right + 5, currentY - 2);

            currentY = Math.Max(txtGamePath.Bottom, cmbLanguages.Bottom) + spacing * 2;

            // Content area
            lblRadios.Location = new Point(margin, currentY);
            lblSongs.Location = new Point(margin + lstRadios.Width + spacing, currentY);
            currentY += lblRadios.Height + 5;

            lstRadios.Location = new Point(margin, currentY);
            
            dgvSongs.Location = new Point(lstRadios.Right + spacing, currentY);
            dgvSongs.Width = ClientSize.Width - dgvSongs.Left - margin;

            // Calculate button row Y position
            int buttonRowY = ClientSize.Height - btnExit.Height - margin - progressBar.Height - spacing;
            
            // Adjust grid height
            dgvSongs.Height = buttonRowY - dgvSongs.Top - spacing;
            lstRadios.Height = dgvSongs.Height;

            // Button row
            btnExit.Location = new Point(ClientSize.Width - btnExit.Width - margin, buttonRowY);
            btnApplyChanges.Location = new Point(btnExit.Left - btnApplyChanges.Width - spacing, buttonRowY);
            btnInsertIntoGame.Location = new Point(btnApplyChanges.Left - btnInsertIntoGame.Width - spacing, buttonRowY);
            btnExtractBank.Location = new Point(margin, buttonRowY);

            // Status bar
            progressBar.Location = new Point(margin, buttonRowY + btnExit.Height + 5);
            progressBar.Width = ClientSize.Width - margin * 2;
            
            lblStatus.Location = new Point(margin, progressBar.Bottom + 2);
        }

        private void SetupEventHandlers()
        {
            btnBrowseGamePath.Click += OnBrowseGamePath;
            cmbLanguages.SelectedIndexChanged += OnLanguageChanged;
            lstRadios.SelectedIndexChanged += OnRadioSelectionChanged;
            dgvSongs.CellContentClick += OnSongGridCellContentClick;
            dgvSongs.CellValueChanged += OnSongGridCellValueChanged;
            btnApplyChanges.Click += OnApplyChanges;
            btnExtractBank.Click += OnExtractBank;
            btnInsertIntoGame.Click += OnInsertIntoGame;
            btnExit.Click += (s, e) => Close();
            Resize += OnFormResize;
            FormClosing += OnFormClosing;
        }

        private async void OnBrowseGamePath(object? sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = Strings.Get(_currentLanguage, "SeleccionaCarpeta"),
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            await SetGamePathAsync(dialog.SelectedPath);
        }

        private void LoadCustomIcon()
        {
            try
            {
                _logger.LogInformation("🎨 Intentando cargar icono personalizado...");
                
                // Ruta al PNG original
                var pngPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "cuadro con un auto d.png");
                var pngPathAlt = Path.Combine(Application.StartupPath, "cuadro con un auto d.png");
                
                string? foundPngPath = null;
                
                if (File.Exists(pngPath))
                {
                    foundPngPath = pngPath;
                }
                else if (File.Exists(pngPathAlt))
                {
                    foundPngPath = pngPathAlt;
                }
                else
                {
                    // Buscar en directorio actual
                    var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.png", SearchOption.AllDirectories)
                        .Where(f => Path.GetFileName(f).Contains("auto"))
                        .FirstOrDefault();
                    
                    if (files != null)
                        foundPngPath = files;
                }
                
                if (foundPngPath != null && File.Exists(foundPngPath))
                {
                    _logger.LogInformation("📷 PNG encontrado en: {PngPath}", foundPngPath);
                    
                    // Cargar PNG y convertir a Icon
                    using var originalBitmap = new Bitmap(foundPngPath);
                    
                    // Crear bitmap de 32x32 con alta calidad
                    using var iconBitmap = new Bitmap(32, 32, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    using var graphics = Graphics.FromImage(iconBitmap);
                    
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    
                    graphics.DrawImage(originalBitmap, 0, 0, 32, 32);
                    
                    // Convertir a Icon
                    var hIcon = iconBitmap.GetHicon();
                    Icon = Icon.FromHandle(hIcon);
                    
                    _logger.LogInformation("✅ Icono personalizado cargado y aplicado exitosamente");
                }
                else
                {
                    _logger.LogWarning("❌ No se encontró la imagen PNG. Usando icono por defecto");
                    Icon = SystemIcons.Application;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error cargando icono personalizado: {Error}", ex.Message);
                Icon = SystemIcons.Application;
            }
        }
    }
}