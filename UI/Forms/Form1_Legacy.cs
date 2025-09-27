using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using ForzaRadioModTool.Helpers;

namespace ForzaRadioModTool
{
    public partial class Form1 : Form
    {
        private TextBox txtCarpetaJuego = null!;
        private ListBox lstRadios = null!;
        private Label lblRadios = null!;
        private Label lblCanciones = null!;
        private Label lblDirectorioJuego = null!;
        private ComboBox cmbIdiomas = null!;
        private Label lblIdioma = null!; // NUEVO
        private Button btnSalir = null!;
        private Button btnAplicarCambios = null!;
        private Button btnAbrirExtractor = null!;
        private Button btnInsertarEnJuego = null!;
        private Button btnBuscarCarpeta = null!;
        private DataGridView dgvCanciones = null!;

        // Evita ejecutar lógica de UI durante la construcción del formulario
        private bool _initializing = false;

        private Dictionary<string, XmlDocument> xmlDocuments = new Dictionary<string, XmlDocument>();
        private string idiomaActual = "EN";
        private string carpetaJuego = "";

        // Using shared radio configuration instead of local mapping
        // See Core/Models/RadioConfiguration.cs for the centralized mapping

        private const string MEDIA_PATH = "media";
        private const string AUDIO_PATH = "Audio";
        private const string FMODBANKS_PATH = "FMODBanks";

        public Form1()
        {
            this.Text = Strings.Get(idiomaActual, "AppTitle");
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(1000, 600);
            this.MinimumSize = new Size(1000, 600);

            _initializing = true; // INICIO de inicialización

            lblDirectorioJuego = new Label()
            {
                Left = 20,
                Top = 0,
                Width = 250,
                Height = 20,
                Text = Strings.Get(idiomaActual, "DirectorioJuego"),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            this.Controls.Add(lblDirectorioJuego);

            txtCarpetaJuego = new TextBox()
            {
                Left = 20,
                Top = 20,
                Width = 500,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(txtCarpetaJuego);

            btnBuscarCarpeta = new Button()
            {
                Width = 160,
                Height = 40,
                Top = 16,
                Text = Strings.Get(idiomaActual, "BuscarCarpeta"),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnBuscarCarpeta.Click += BtnBuscarCarpeta_Click;
            this.Controls.Add(btnBuscarCarpeta);

            cmbIdiomas = new ComboBox()
            {
                Width = 110,
                Top = 20,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            cmbIdiomas.Items.AddRange(new string[] { "EN", "MX", "BR", "DE", "FR" });
            cmbIdiomas.SelectedIndexChanged += CmbIdiomas_SelectedIndexChanged;
            cmbIdiomas.SelectedIndex = 0;
            cmbIdiomas.Enabled = false;
            this.Controls.Add(cmbIdiomas);

            // NUEVO: etiqueta sobre el ComboBox de idioma
            lblIdioma = new Label()
            {
                AutoSize = true,
                Text = Strings.Get(idiomaActual, "Idioma"),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            this.Controls.Add(lblIdioma);

            lblRadios = new Label()
            {
                Left = 20,
                Top = 70,
                Width = 260,
                Height = 25,
                Text = Strings.Get(idiomaActual, "ListaRadios"),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            this.Controls.Add(lblRadios);

            lstRadios = new ListBox()
            {
                Left = 20,
                Top = lblRadios.Bottom + 5,
                Width = 260,
                Height = 400,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
            };
            lstRadios.SelectedIndexChanged += LstRadios_SelectedIndexChanged;
            lstRadios.Enabled = false;
            this.Controls.Add(lstRadios);

            lblCanciones = new Label()
            {
                Left = 290,
                Top = 70,
                Width = 300,
                Height = 25,
                Text = Strings.Get(idiomaActual, "CancionesRadio"),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            this.Controls.Add(lblCanciones);

            dgvCanciones = new DataGridView()
            {
                Left = 290,
                Top = lblCanciones.Bottom + 5,
                Width = this.ClientSize.Width - 310,
                Height = 400,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            dgvCanciones.RowHeadersVisible = false;
            dgvCanciones.Columns.Add(new DataGridViewButtonColumn { HeaderText = "▶", Text = Strings.Get(idiomaActual, "Play"), UseColumnTextForButtonValue = true, Width = 60 });
            dgvCanciones.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = Strings.Get(idiomaActual, "ColID"), ReadOnly = true, Width = 50 });
            dgvCanciones.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = Strings.Get(idiomaActual, "ColArchivo"), ReadOnly = true });
            dgvCanciones.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = Strings.Get(idiomaActual, "ColNombre") });
            dgvCanciones.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = Strings.Get(idiomaActual, "ColArtista") });
            dgvCanciones.Columns.Add(new DataGridViewButtonColumn { HeaderText = Strings.Get(idiomaActual, "ReemplazarCancion"), Text = Strings.Get(idiomaActual, "ReemplazarCancion"), UseColumnTextForButtonValue = true });
            dgvCanciones.CellContentClick += DgvCanciones_CellContentClick;
            dgvCanciones.Enabled = false;
            this.Controls.Add(dgvCanciones);

            btnAplicarCambios = new Button()
            {
                Width = 170,
                Height = 40,
                Text = Strings.Get(idiomaActual, "AplicarCambios"),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnAplicarCambios.Click += BtnAplicarCambios_Click;
            btnAplicarCambios.Enabled = false;
            this.Controls.Add(btnAplicarCambios);

            btnSalir = new Button()
            {
                Width = 120,
                Height = 40,
                Text = Strings.Get(idiomaActual, "Salir"),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnSalir.Click += (s, e) => this.Close();
            this.Controls.Add(btnSalir);

            btnAbrirExtractor = new Button()
            {
                Left = 20,
                Width = 180,
                Height = 40,
                Text = Strings.Get(idiomaActual, "RunFMOD"),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnAbrirExtractor.Click += BtnExtraerBank_Click;
            this.Controls.Add(btnAbrirExtractor);

            btnInsertarEnJuego = new Button()
            {
                Width = 250,
                Height = 40,
                Text = Strings.Get(idiomaActual, "InsertarEnJuego"),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnInsertarEnJuego.Click += BtnInsertarEnJuego_Click;
            this.Controls.Add(btnInsertarEnJuego);

            this.Resize += Form1_Resize;
            Form1_Resize(this, EventArgs.Empty);

            _initializing = false; // FIN de inicialización
        }

        private void HabilitarControles(bool habilitar)
        {
            lstRadios.Enabled = habilitar;
            dgvCanciones.Enabled = habilitar;
            cmbIdiomas.Enabled = habilitar;
            btnAplicarCambios.Enabled = habilitar;
            btnAbrirExtractor.Enabled = habilitar;
            btnInsertarEnJuego.Enabled = habilitar;
        }

        private void BtnBuscarCarpeta_Click(object? sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = Strings.Get(idiomaActual, "SeleccionaCarpeta");
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    carpetaJuego = dialog.SelectedPath;
                    txtCarpetaJuego.Text = carpetaJuego;

                    lstRadios.Items.Clear();
                    dgvCanciones.Rows.Clear();
                    xmlDocuments.Clear();
                    HabilitarControles(false);

                    try
                    {
                        foreach (string idioma in cmbIdiomas.Items)
                        {
                            string xmlPath = Path.Combine(carpetaJuego, MEDIA_PATH, AUDIO_PATH, $"RadioInfo_{idioma}.xml");
                            if (File.Exists(xmlPath))
                            {
                                XmlDocument doc = new XmlDocument();
                                string xmlContent = File.ReadAllText(xmlPath);
                                xmlContent = xmlContent.Replace("&", "&amp;");
                                doc.LoadXml(xmlContent);
                                xmlDocuments[idioma.ToString()] = doc;
                            }
                        }

                        CargarRadios();

                        if (lstRadios.Items.Count > 0)
                        {
                            HabilitarControles(true);
                        }
                        else
                        {
                            MessageBox.Show(Strings.Get(idiomaActual, "NoRadios"));
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"{Strings.Get(idiomaActual, "ErrorCargar")}: {ex.Message}");
                    }
                }
            }
        }

        private void CargarRadios()
        {
            lstRadios.Items.Clear();
            if (string.IsNullOrEmpty(carpetaJuego)) return;
            if (!xmlDocuments.ContainsKey(idiomaActual)) return;

            XmlDocument doc = xmlDocuments[idiomaActual];
            string fmodBanksPath = Path.Combine(carpetaJuego, MEDIA_PATH, AUDIO_PATH, FMODBANKS_PATH);
            if (Directory.Exists(fmodBanksPath))
            {
                foreach (var pair in radiosMap)
                {
                    string bankFile = pair.Key;
                    foreach (string radioName in pair.Value)
                    {
                        string bankPath = Path.Combine(fmodBanksPath, bankFile);
                        if (File.Exists(bankPath))
                        {
                            XmlNode? radioNode = doc.SelectSingleNode($"/Radio/RadioStations/RadioStation[@Name='{radioName}']");
                            if (radioNode != null)
                            {
                                lstRadios.Items.Add(radioName);
                            }
                        }
                    }
                }
                lstRadios.Enabled = lstRadios.Items.Count > 0;
                if (lstRadios.Items.Count > 0)
                {
                    lstRadios.SelectedIndex = 0;
                }
            }
            else
            {
                MessageBox.Show(Strings.Get(idiomaActual, "NoFMODBanks"));
            }
        }

        private void LstRadios_SelectedIndexChanged(object? sender, EventArgs e)
        {
            CargarCanciones();
        }

        private void GenerarTxtDesdeFsproj(string fsprojPath, string txtPath)
        {
            var nombres = new List<string>();
            if (File.Exists(fsprojPath))
            {
                var doc = new XmlDocument();
                doc.Load(fsprojPath);
                var subsounds = doc.SelectNodes("//subsound/file");
                if (subsounds != null)
                {
                    foreach (XmlNode node in subsounds)
                    {
                        string file = node.InnerText.Trim();
                        string wavName = Path.GetFileName(file);
                        if (!string.IsNullOrEmpty(wavName))
                            nombres.Add(wavName);
                    }
                }
            }
            if (nombres.Count > 0)
                File.WriteAllLines(txtPath, nombres);
        }

        private void CargarCanciones()
        {
            dgvCanciones.Rows.Clear();
            if (lstRadios.SelectedItem == null) return;
            if (!xmlDocuments.TryGetValue(idiomaActual, out XmlDocument? doc) || doc == null) return;

            string radioName = lstRadios.SelectedItem.ToString() ?? "";
            XmlNode? selectedRadio = doc.SelectSingleNode($"/Radio/RadioStations/RadioStation[@Name='{radioName}']");

            string bankFile = ForzaRadioModTool.Core.Models.RadioConfiguration.GetBankForRadio(radioName) ?? "";
            string appFolder = Application.StartupPath;
            string bankFolder = Path.GetFileNameWithoutExtension(bankFile);
            string fsprojPath = Path.Combine(appFolder, "Tools", "FmodBankTools", "fsb", $"{bankFile}.fsproj");
            string txtPath = Path.Combine(appFolder, "Tools", "FmodBankTools", "wav", bankFolder, $"{bankFolder}.txt");

            try { GenerarTxtDesdeFsproj(fsprojPath, txtPath); } catch {}

            var wavNames = new List<string>();
            if (File.Exists(txtPath))
            {
                wavNames = File.ReadAllLines(txtPath).Select(line => Path.GetFileNameWithoutExtension(line.Trim())).Where(n => !string.IsNullOrEmpty(n)).ToList();
            }

            if (selectedRadio != null)
            {
                XmlNodeList? samples = selectedRadio.SelectNodes("SampleList/*");
                if (samples != null)
                {
                    for (int i = 0; i < samples.Count && i < wavNames.Count; i++)
                    {
                        XmlNode? sample = samples[i];
                        if (sample == null) continue;

                        string fileName = "";
                        string name = "";
                        string artist = "";
                        string wavId = wavNames[i];

                        if (sample.LocalName == "Sample")
                        {
                            fileName = sample.Attributes?["SoundName"]?.Value ?? "";
                            name = sample.Attributes?["DisplayName"]?.Value ?? "";
                            artist = sample.Attributes?["Artist"]?.Value ?? "";
                        }
                        else if (sample.LocalName == "Entry")
                        {
                            fileName = sample.Attributes?["Name"]?.Value ?? "";
                            string entryName = sample.Attributes?["Name"]?.Value ?? "";
                            string[] parts = entryName.Split('_');
                            if (parts.Length > 2)
                            {
                                name = string.Join(" ", parts.Skip(2));
                                artist = "Various Artists";
                            }
                        }

                        int rowIndex = dgvCanciones.Rows.Add(Strings.Get(idiomaActual, "Play"), wavId, fileName, name, artist);
                        // Guardar referencia al nodo XML
                        dgvCanciones.Rows[rowIndex].Tag = sample;
                    }
                }
            }
        }

        private void DgvCanciones_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (e.ColumnIndex == 0)
            {
                string wavId = dgvCanciones.Rows[e.RowIndex].Cells[1].Value?.ToString() ?? "";
                string radioName = lstRadios.SelectedItem?.ToString() ?? "";
                string bankFile = ForzaRadioModTool.Core.Models.RadioConfiguration.GetBankForRadio(radioName) ?? "";
                string appFolder = Application.StartupPath;
                string bankFolder = Path.GetFileNameWithoutExtension(bankFile);
                string wavFolder = Path.Combine(appFolder, "Tools", "FmodBankTools", "wav", bankFolder);
                string filePath = Path.Combine(wavFolder, $"{wavId}.wav");

                if (File.Exists(filePath))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                        {
                            FileName = filePath,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"{Strings.Get(idiomaActual, "NoSePudoAbrirArchivo")}: {ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show(Strings.Get(idiomaActual, "NoWavParaPreview"));
                }
            }
            else if (e.ColumnIndex == dgvCanciones.Columns.Count - 1)
            {
                string wavId = dgvCanciones.Rows[e.RowIndex].Cells[1].Value?.ToString() ?? "";
                string radioName = lstRadios.SelectedItem?.ToString() ?? "";
                string bankFile = ForzaRadioModTool.Core.Models.RadioConfiguration.GetBankForRadio(radioName) ?? "";
                string appFolder = Application.StartupPath;
                string bankFolder = Path.GetFileNameWithoutExtension(bankFile);
                string wavFolder = Path.Combine(appFolder, "Tools", "FmodBankTools", "wav", bankFolder);
                Directory.CreateDirectory(wavFolder);
                string filePath = Path.Combine(wavFolder, $"{wavId}.wav");

                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Title = Strings.Get(idiomaActual, "SeleccionaNuevoAudio");
                    ofd.Filter = "Audio|*.wav;*.mp3;*.ogg;*.flac;*.aac;*.m4a;*.wma|All|*.*";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            // Siempre convierte a WAV 44.1kHz estéreo
                            if (!AudioUtil.ResampleToWav441k(ofd.FileName, filePath, -13))
                                throw new Exception("ffmpeg conversion failed or ffmpeg.exe not found.");

                            // Lee el WAV resultante
                            if (AudioUtil.TryGetWavInfo(filePath, out int finalRate, out _, out _, out long frames))
                            {
                                var row = dgvCanciones.Rows[e.RowIndex];
                                if (row.Tag is XmlNode xmlSample && xmlSample.LocalName == "Sample")
                                {
                                    // SampleRate fijo a 44100
                                    var rateAttr = xmlSample.Attributes?["SampleRate"];
                                    if (rateAttr == null)
                                    {
                                        rateAttr = xmlSample.OwnerDocument!.CreateAttribute("SampleRate");
                                        xmlSample.Attributes!.Append(rateAttr);
                                    }
                                    rateAttr.Value = "44100";

                                    // SampleLength = frames a 44.1kHz
                                    var lenAttr = xmlSample.Attributes?["SampleLength"];
                                    if (lenAttr == null)
                                    {
                                        lenAttr = xmlSample.OwnerDocument!.CreateAttribute("SampleLength");
                                        xmlSample.Attributes!.Append(lenAttr);
                                    }
                                    lenAttr.Value = frames.ToString();

                                    // Ajustar marcador End = SampleLength - 1 (si existe)
                                    var endMarker = xmlSample.SelectSingleNode("Marker[@Name='End']") as XmlElement;
                                    if (endMarker != null)
                                    {
                                        endMarker.SetAttribute("Position", Math.Max(0, frames - 1).ToString());
                                    }
                                }
                            }

                            MessageBox.Show(Strings.Get(idiomaActual, "CancionReemplazadaOk"));
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"{Strings.Get(idiomaActual, "ErrorReemplazarCancion")}: {ex.Message}");
                        }
                    }
                }
            }
        }

        private void BtnAplicarCambios_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(
                Strings.Get(idiomaActual, "InstrRebuildBody"),
                Strings.Get(idiomaActual, "InstrRebuildTitle"),
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (lstRadios.SelectedItem == null) return;
            if (!xmlDocuments.TryGetValue(idiomaActual, out XmlDocument? doc) || doc == null) return;

            string radioName = lstRadios.SelectedItem.ToString() ?? "";
            XmlNode? selectedRadio = doc.SelectSingleNode($"/Radio/RadioStations/RadioStation[@Name='{radioName}']");

            if (selectedRadio != null)
            {
                XmlNodeList? xmlSamples = selectedRadio.SelectNodes("SampleList/*");
                if (xmlSamples != null)
                {
                    for (int i = 0; i < dgvCanciones.Rows.Count && i < xmlSamples.Count; i++)
                    {
                        var row = dgvCanciones.Rows[i];
                        XmlNode? xmlSample = xmlSamples[i];

                        if (xmlSample?.LocalName == "Sample")
                        {
                            if (xmlSample.Attributes?["DisplayName"] != null)
                                xmlSample.Attributes["DisplayName"]!.Value = row.Cells[3].Value?.ToString() ?? "";
                            if (xmlSample.Attributes?["Artist"] != null)
                                xmlSample.Attributes["Artist"]!.Value = row.Cells[4].Value?.ToString() ?? "";
                        }
                    }
                }

                string filePath = Path.Combine(carpetaJuego, MEDIA_PATH, AUDIO_PATH, $"RadioInfo_{idiomaActual}.xml");
                try
                {
                    doc.Save(filePath);
                    string xmlContent = File.ReadAllText(filePath);
                    xmlContent = xmlContent.Replace("&amp;", "&");
                    File.WriteAllText(filePath, xmlContent);
                    MessageBox.Show(Strings.Get(idiomaActual, "CambiosAplicados"));
                    CargarCanciones();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{Strings.Get(idiomaActual, "ErrorGuardar")}: {ex.Message}");
                }
            }
        }

        private void BtnExtraerBank_Click(object? sender, EventArgs e)
        {
            if (lstRadios.SelectedItem == null)
            {
                MessageBox.Show(Strings.Get(idiomaActual, "SeleccionaRadioPrimero"));
                return;
            }

            string radioName = lstRadios.SelectedItem.ToString() ?? "";
            string bankFile = ForzaRadioModTool.Core.Models.RadioConfiguration.GetBankForRadio(radioName) ?? "";
            if (string.IsNullOrEmpty(bankFile)) return;

            string appFolder = Application.StartupPath;
            string backupFolder = Path.Combine(appFolder, "Tools", "FmodBankTools", "bank");
            Directory.CreateDirectory(backupFolder);

            string banksPath = Path.Combine(carpetaJuego, MEDIA_PATH, AUDIO_PATH, FMODBANKS_PATH);

            foreach (var pair in radiosMap)
            {
                string bank = pair.Key;
                string source = Path.Combine(banksPath, bank);
                string dest = Path.Combine(backupFolder, bank);

                if (File.Exists(source))
                {
                    try
                    {
                        File.Copy(source, dest, true);
                    }
                    catch { }
                }
            }

            string destFile = Path.Combine(backupFolder, bankFile);

            string wavFolder = Path.Combine(appFolder, "Tools", "FmodBankTools", "wav", Path.GetFileNameWithoutExtension(bankFile));
            Directory.CreateDirectory(wavFolder);

            if (File.Exists(destFile))
            {
                MessageBox.Show(
                    Strings.Get(idiomaActual, "InstrFMODBody"),
                    Strings.Get(idiomaActual, "InstrFMODTitle"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                try
                {
                    var process = new System.Diagnostics.Process();
                    process.StartInfo.FileName = Path.Combine(appFolder, "Tools", "FmodBankTools", "Fmod Bank Tools.exe");
                    process.StartInfo.Arguments = $"-format vorbis -quality 90 -o \"{wavFolder}\" \"{destFile}\"";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.EnableRaisingEvents = true;
                    process.Exited += (s, args) =>
                    {
                        this.Invoke((Action)(() =>
                        {
                            string? radioSeleccionada = lstRadios.SelectedItem?.ToString();
                            CargarRadios();
                            if (!string.IsNullOrEmpty(radioSeleccionada))
                            {
                                int idx = lstRadios.Items.IndexOf(radioSeleccionada);
                                if (idx >= 0) lstRadios.SelectedIndex = idx;
                            }
                            CargarCanciones();
                        }));
                    };
                    process.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{Strings.Get(idiomaActual, "ErrorExtraer")}: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show(Strings.Get(idiomaActual, "NoBankBackup"));
            }
        }

        private void BtnInsertarEnJuego_Click(object? sender, EventArgs e)
        {
            string appFolder = Application.StartupPath;
            string fmodBankFolder = Path.Combine(appFolder, "Tools", "FmodBankTools", "bank");
            string juegoBankFolder = Path.Combine(carpetaJuego, MEDIA_PATH, AUDIO_PATH, FMODBANKS_PATH);

            int copiados = 0;
            foreach (var pair in radiosMap)
            {
                string bankFile = pair.Key;
                string source = Path.Combine(fmodBankFolder, bankFile);
                string dest = Path.Combine(juegoBankFolder, bankFile);

                if (File.Exists(source))
                {
                    try
                    {
                        File.Copy(source, dest, true);
                        copiados++;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format(Strings.Get(idiomaActual, "ErrorCopiandoBank"), bankFile) + $": {ex.Message}");
                    }
                }
            }

            MessageBox.Show(string.Format(Strings.Get(idiomaActual, "CopiadosAlJuego"), copiados),
                Strings.Get(idiomaActual, "InsertarEnJuegoTitulo"),
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CmbIdiomas_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_initializing) return;

            idiomaActual = cmbIdiomas.SelectedItem?.ToString() ?? "EN";

            this.Text = Strings.Get(idiomaActual, "AppTitle");
            lblDirectorioJuego.Text = Strings.Get(idiomaActual, "DirectorioJuego");
            btnBuscarCarpeta.Text = Strings.Get(idiomaActual, "BuscarCarpeta");
            lblRadios.Text = Strings.Get(idiomaActual, "ListaRadios");
            lblCanciones.Text = Strings.Get(idiomaActual, "CancionesRadio");
            btnAplicarCambios.Text = Strings.Get(idiomaActual, "AplicarCambios");
            btnSalir.Text = Strings.Get(idiomaActual, "Salir");
            btnAbrirExtractor.Text = Strings.Get(idiomaActual, "RunFMOD");
            btnInsertarEnJuego.Text = Strings.Get(idiomaActual, "InsertarEnJuego");
            lblIdioma.Text = Strings.Get(idiomaActual, "Idioma");   // NUEVO

            if (dgvCanciones != null && dgvCanciones.Columns.Count >= 6)
            {
                ((DataGridViewButtonColumn)dgvCanciones.Columns[0]).Text = Strings.Get(idiomaActual, "Play");
                dgvCanciones.Columns[1].HeaderText = Strings.Get(idiomaActual, "ColID");
                dgvCanciones.Columns[2].HeaderText = Strings.Get(idiomaActual, "ColArchivo");
                dgvCanciones.Columns[3].HeaderText = Strings.Get(idiomaActual, "ColNombre");
                dgvCanciones.Columns[4].HeaderText = Strings.Get(idiomaActual, "ColArtista");
                dgvCanciones.Columns[5].HeaderText = Strings.Get(idiomaActual, "ReemplazarCancion");
                ((DataGridViewButtonColumn)dgvCanciones.Columns[5]).Text = Strings.Get(idiomaActual, "ReemplazarCancion");
            }

            Form1_Resize(this, EventArgs.Empty); // Reposicionar el label de idioma
            CargarRadios();
            CargarCanciones();
        }

        private void Form1_Resize(object? sender, EventArgs e)
        {
            int margin = 20;
            int gap = 10;

            // Arriba-derecha
            if (cmbIdiomas != null)
            {
                cmbIdiomas.Left = this.ClientSize.Width - margin - cmbIdiomas.Width;
                cmbIdiomas.Top = 20;
            }
            if (lblIdioma != null && cmbIdiomas != null)
            {
                lblIdioma.Top = 0;                         // NUEVO: arriba del combo
                lblIdioma.Left = cmbIdiomas.Right - lblIdioma.Width;
            }
            if (btnBuscarCarpeta != null && cmbIdiomas != null)
            {
                btnBuscarCarpeta.Left = cmbIdiomas.Left - gap - btnBuscarCarpeta.Width;
                btnBuscarCarpeta.Top = 16;
            }

            // Primero posiciona los botones inferiores
            if (btnSalir != null)
            {
                btnSalir.Left = this.ClientSize.Width - margin - btnSalir.Width;
                btnSalir.Top = this.ClientSize.Height - margin - btnSalir.Height;
            }
            if (btnAplicarCambios != null && btnSalir != null)
            {
                btnAplicarCambios.Left = btnSalir.Left - gap - btnAplicarCambios.Width;
                btnAplicarCambios.Top = btnSalir.Top;
            }
            if (btnAbrirExtractor != null && btnSalir != null)
            {
                btnAbrirExtractor.Left = 20;
                btnAbrirExtractor.Top = btnSalir.Top;
            }
            if (btnInsertarEnJuego != null && btnAbrirExtractor != null)
            {
                btnInsertarEnJuego.Left = btnAbrirExtractor.Right + gap;
                btnInsertarEnJuego.Top = btnAbrirExtractor.Top;
            }

            // Ahora ajusta la grilla con el Top correcto de btnSalir
            if (dgvCanciones != null)
            {
                dgvCanciones.Width = this.ClientSize.Width - dgvCanciones.Left - margin;

                int bottomLimit = (btnSalir != null ? btnSalir.Top : this.ClientSize.Height - margin);
                int desiredHeight = bottomLimit - dgvCanciones.Top - gap;
                if (desiredHeight < 120) desiredHeight = 120; // altura mínima para que se vea
                dgvCanciones.Height = desiredHeight;
            }
        }

        
    }
}