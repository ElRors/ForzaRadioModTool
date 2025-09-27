        private async Task SetGamePathAsync(string gamePath)
        {
            try
            {
                await SetStatusAsync("Validating game path...", true);

                _logger.LogInformation("Setting game path to: {GamePath}", gamePath);

                var validation = _fileManager.ValidateGamePath(gamePath);
                if (!validation.IsSuccess)
                {
                    await ShowErrorAsync(validation.ErrorMessage);
                    return;
                }

                _currentGamePath = gamePath;
                txtGamePath.Text = gamePath;

                // Load available languages
                var languagesResult = await _fileManager.GetAvailableLanguagesAsync(gamePath);
                if (!languagesResult.IsSuccess)
                {
                    await ShowErrorAsync($"Failed to load languages: {languagesResult.ErrorMessage}");
                    return;
                }

                var languages = languagesResult.Data!.ToList();
                if (!languages.Any())
                {
                    await ShowErrorAsync("No supported language files found in game directory.");
                    return;
                }

                cmbLanguages.Items.Clear();
                cmbLanguages.Items.AddRange(languages.ToArray());
                cmbLanguages.SelectedItem = languages.Contains(_currentLanguage) ? _currentLanguage : languages.First();
                cmbLanguages.Enabled = true;

                await LoadRadiosAsync();
                await SetStatusAsync("Game path loaded successfully", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting game path: {GamePath}", gamePath);
                await ShowErrorAsync($"Error setting game path: {ex.Message}");
            }
        }

        private async void OnLanguageChanged(object? sender, EventArgs e)
        {
            if (_isInitializing || cmbLanguages.SelectedItem == null)
                return;

            var newLanguage = cmbLanguages.SelectedItem.ToString()!;
            if (newLanguage == _currentLanguage)
                return;

            _currentLanguage = newLanguage;
            ApplyLanguage();
            
            if (!string.IsNullOrEmpty(_currentGamePath))
            {
                await LoadRadiosAsync();
            }
        }

        private async void OnRadioSelectionChanged(object? sender, EventArgs e)
        {
            if (lstRadios.SelectedItem == null)
            {
                dgvSongs.Rows.Clear();
                _currentSongs.Clear();
                EnableSongControls(false);
                return;
            }

            await LoadSongsAsync(lstRadios.SelectedItem.ToString()!);
        }

        private async void OnSongGridCellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _currentSongs.Count)
                return;

            var song = _currentSongs[e.RowIndex];

            try
            {
                if (e.ColumnIndex == dgvSongs.Columns["PlayColumn"]!.Index)
                {
                    await PlaySongAsync(song);
                }
                else if (e.ColumnIndex == dgvSongs.Columns["ReplaceColumn"]!.Index)
                {
                    await ReplaceSongAsync(song);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling song grid cell click");
                await ShowErrorAsync($"Error: {ex.Message}");
            }
        }

        private void OnSongGridCellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _currentSongs.Count || _isInitializing)
                return;

            try
            {
                var song = _currentSongs[e.RowIndex];
                var row = dgvSongs.Rows[e.RowIndex];

                var newName = row.Cells["NameColumn"].Value?.ToString() ?? song.Name;
                var newArtist = row.Cells["ArtistColumn"].Value?.ToString() ?? song.Artist;

                _currentSongs[e.RowIndex] = song.WithUpdatedMetadata(newName, newArtist);
                
                _logger.LogInformation("Song metadata updated: {SongId}", song.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating song metadata");
            }
        }

        private async void OnApplyChanges(object? sender, EventArgs e)
        {
            if (lstRadios.SelectedItem == null)
                return;

            try
            {
                await SetStatusAsync("Applying changes...", true);

                var radioName = lstRadios.SelectedItem.ToString()!;

                // Update each song in the radio
                foreach (var song in _currentSongs)
                {
                    var result = await _radioManager.UpdateSongMetadataAsync(radioName, _currentLanguage, song);
                    if (!result.IsSuccess)
                    {
                        await ShowErrorAsync($"Failed to update song {song.Id}: {result.ErrorMessage}");
                        return;
                    }
                }

                // Save changes
                var saveResult = await _radioManager.SaveChangesAsync(radioName, _currentLanguage);
                if (!saveResult.IsSuccess)
                {
                    await ShowErrorAsync($"Failed to save changes: {saveResult.ErrorMessage}");
                    return;
                }

                await ShowInfoAsync("Changes applied successfully!");
                await LoadSongsAsync(radioName); // Refresh display
                await SetStatusAsync("Changes applied successfully", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying changes");
                await ShowErrorAsync($"Error applying changes: {ex.Message}");
            }
        }

        private async void OnExtractBank(object? sender, EventArgs e)
        {
            if (lstRadios.SelectedItem == null)
                return;

            try
            {
                await SetStatusAsync("Extracting bank files...", true);

                var radioName = lstRadios.SelectedItem.ToString()!;
                var bankFileResult = _radioManager.GetBankFileForRadio(radioName);
                
                if (!bankFileResult.IsSuccess)
                {
                    await ShowErrorAsync(bankFileResult.ErrorMessage);
                    return;
                }

                var bankFile = bankFileResult.Data!;
                var bankPath = Path.Combine(_currentGamePath!, "media", "Audio", "FMODBanks", bankFile);
                var extractPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "FmodBankTools", "wav", 
                    Path.GetFileNameWithoutExtension(bankFile));

                // Backup bank files first
                var backupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "FmodBankTools", "bank");
                var backupResult = await _fileManager.BackupBankFilesAsync(_currentGamePath!, backupPath);
                
                if (!backupResult.IsSuccess)
                {
                    _logger.LogWarning("Failed to backup bank files: {Error}", backupResult.ErrorMessage);
                }

                // Extract bank
                var extractResult = await _audioProcessor.ExtractBankAsync(bankPath, extractPath);
                if (!extractResult.IsSuccess)
                {
                    await ShowErrorAsync($"Failed to extract bank: {extractResult.ErrorMessage}");
                    return;
                }

                await ShowInfoAsync("Bank extracted successfully! You can now modify the audio files and rebuild the bank using FMOD tools.");
                await LoadSongsAsync(radioName); // Refresh to show local files
                await SetStatusAsync("Bank extraction completed", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting bank");
                await ShowErrorAsync($"Error extracting bank: {ex.Message}");
            }
        }

        private async void OnInsertIntoGame(object? sender, EventArgs e)
        {
            try
            {
                await SetStatusAsync("Inserting modified files into game...", true);

                var sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "FmodBankTools", "bank");
                var result = await _fileManager.CopyBankFilesToGameAsync(sourcePath, _currentGamePath!);

                if (!result.IsSuccess)
                {
                    await ShowErrorAsync($"Failed to insert files: {result.ErrorMessage}");
                    return;
                }

                await ShowInfoAsync("Modified files inserted into game successfully!");
                await SetStatusAsync("Files inserted successfully", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting files into game");
                await ShowErrorAsync($"Error inserting files: {ex.Message}");
            }
        }

        private void OnFormResize(object? sender, EventArgs e)
        {
            if (_isInitializing)
                return;

            LayoutControls();
        }

        private void OnFormClosing(object? sender, FormClosingEventArgs e)
        {
            _logger.LogInformation("MainForm closing");
        }

        private async Task LoadRadiosAsync()
        {
            if (string.IsNullOrEmpty(_currentGamePath))
                return;

            try
            {
                await SetStatusAsync("Loading radio stations...", true);

                var result = await _radioManager.LoadRadiosAsync(_currentGamePath, _currentLanguage);
                if (!result.IsSuccess)
                {
                    await ShowErrorAsync($"Failed to load radios: {result.ErrorMessage}");
                    return;
                }

                _currentRadios = result.Data!.ToList();
                
                lstRadios.Items.Clear();
                lstRadios.Items.AddRange(_currentRadios.Select(r => r.Name).ToArray());
                
                var hasRadios = _currentRadios.Any();
                EnableRadioControls(hasRadios);

                if (hasRadios)
                {
                    lstRadios.SelectedIndex = 0;
                }

                await SetStatusAsync($"Loaded {_currentRadios.Count} radio stations", false);
                _logger.LogInformation("Loaded {Count} radio stations", _currentRadios.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading radios");
                await ShowErrorAsync($"Error loading radios: {ex.Message}");
            }
        }

        private async Task LoadSongsAsync(string radioName)
        {
            try
            {
                await SetStatusAsync($"Loading songs for {radioName}...", true);

                var result = await _radioManager.LoadSongsAsync(radioName, _currentLanguage);
                if (!result.IsSuccess)
                {
                    await ShowErrorAsync($"Failed to load songs: {result.ErrorMessage}");
                    return;
                }

                _currentSongs = result.Data!.ToList();
                await PopulateSongGridAsync(_currentSongs);
                
                var hasSongs = _currentSongs.Any();
                EnableSongControls(hasSongs);

                await SetStatusAsync($"Loaded {_currentSongs.Count} songs for {radioName}", false);
                _logger.LogInformation("Loaded {Count} songs for radio {RadioName}", _currentSongs.Count, radioName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading songs for radio: {RadioName}", radioName);
                await ShowErrorAsync($"Error loading songs: {ex.Message}");
            }
        }

        private async Task PopulateSongGridAsync(List<SongInfo> songs)
        {
            dgvSongs.Rows.Clear();

            foreach (var song in songs)
            {
                var playText = song.HasLocalFile ? "▶" : "○";
                dgvSongs.Rows.Add(playText, song.WavId, song.Name, song.Artist, "Replace");
            }

            await Task.CompletedTask;
        }

        private async Task PlaySongAsync(SongInfo song)
        {
            try
            {
                // Determine file path
                var bankFileResult = _radioManager.GetBankFileForRadio(lstRadios.SelectedItem?.ToString() ?? "");
                if (!bankFileResult.IsSuccess)
                {
                    await ShowErrorAsync("Could not determine bank file for radio");
                    return;
                }

                var bankFolder = Path.GetFileNameWithoutExtension(bankFileResult.Data!);
                var wavPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "FmodBankTools", "wav", bankFolder, $"{song.WavId}.wav");

                if (!File.Exists(wavPath))
                {
                    await ShowInfoAsync("Audio file not found. Please extract the bank first using 'Run FMOD' button.");
                    return;
                }

                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = wavPath,
                    UseShellExecute = true
                };

                System.Diagnostics.Process.Start(startInfo);
                _logger.LogInformation("Playing audio file: {FilePath}", wavPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error playing song: {SongId}", song.Id);
                await ShowErrorAsync($"Could not play audio file: {ex.Message}");
            }
        }

        private async Task ReplaceSongAsync(SongInfo song)
        {
            try
            {
                using var dialog = new OpenFileDialog
                {
                    Title = Strings.Get(_currentLanguage, "SeleccionaNuevoAudio"),
                    Filter = $"Audio Files|{string.Join(";", _settings.Audio.SupportedFormats)}|All Files|*.*",
                    Multiselect = false
                };

                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                var sourceFile = dialog.FileName;

                // Validate audio file
                var validation = _audioProcessor.ValidateAudioFile(sourceFile);
                if (!validation.IsSuccess)
                {
                    await ShowErrorAsync($"Invalid audio file: {validation.ErrorMessage}");
                    return;
                }

                await SetStatusAsync("Converting audio file...", true);

                // Determine target file path
                var bankFileResult = _radioManager.GetBankFileForRadio(lstRadios.SelectedItem?.ToString() ?? "");
                if (!bankFileResult.IsSuccess)
                {
                    await ShowErrorAsync("Could not determine bank file for radio");
                    return;
                }

                var bankFolder = Path.GetFileNameWithoutExtension(bankFileResult.Data!);
                var targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "FmodBankTools", "wav", bankFolder);
                Directory.CreateDirectory(targetPath);
                
                var targetFile = Path.Combine(targetPath, $"{song.WavId}.wav");

                // Convert audio
                var conversionResult = await _audioProcessor.ReplaceAudioAsync(sourceFile, targetFile, _settings.Audio.DefaultVolumeDb);
                if (!conversionResult.IsSuccess)
                {
                    await ShowErrorAsync($"Failed to convert audio: {conversionResult.ErrorMessage}");
                    return;
                }

                // Get audio info for metadata update
                var audioInfo = _audioProcessor.GetAudioInfo(targetFile);
                if (audioInfo.IsSuccess)
                {
                    var (sampleRate, channels, bitsPerSample, sampleFrames) = audioInfo.Data;
                    var updatedSong = song with 
                    { 
                        SampleRate = sampleRate, 
                        SampleLength = sampleFrames,
                        HasLocalFile = true,
                        LocalFilePath = targetFile
                    };

                    // Update in current list
                    var index = _currentSongs.FindIndex(s => s.Id == song.Id);
                    if (index >= 0)
                    {
                        _currentSongs[index] = updatedSong;
                    }

                    // Update XML
                    await _radioManager.UpdateSongMetadataAsync(lstRadios.SelectedItem?.ToString() ?? "", _currentLanguage, updatedSong);
                }

                await ShowInfoAsync("Audio file replaced successfully!");
                await PopulateSongGridAsync(_currentSongs); // Refresh grid
                await SetStatusAsync("Audio file replaced", false);

                _logger.LogInformation("Replaced audio file for song: {SongId}", song.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error replacing song: {SongId}", song.Id);
                await ShowErrorAsync($"Error replacing audio: {ex.Message}");
            }
        }

        private void ApplyLanguage()
        {
            if (_isInitializing)
                return;

            Text = Strings.Get(_currentLanguage, "AppTitle");
            lblGamePath.Text = Strings.Get(_currentLanguage, "DirectorioJuego");
            btnBrowseGamePath.Text = Strings.Get(_currentLanguage, "BuscarCarpeta");
            lblLanguage.Text = Strings.Get(_currentLanguage, "Idioma");
            lblRadios.Text = Strings.Get(_currentLanguage, "ListaRadios");
            lblSongs.Text = Strings.Get(_currentLanguage, "CancionesRadio");
            btnApplyChanges.Text = Strings.Get(_currentLanguage, "AplicarCambios");
            btnExtractBank.Text = Strings.Get(_currentLanguage, "RunFMOD");
            btnInsertIntoGame.Text = Strings.Get(_currentLanguage, "InsertarEnJuego");
            btnExit.Text = Strings.Get(_currentLanguage, "Salir");

            // Update DataGridView columns
            if (dgvSongs.Columns.Count >= 5)
            {
                ((DataGridViewButtonColumn)dgvSongs.Columns["PlayColumn"]).Text = Strings.Get(_currentLanguage, "Play");
                dgvSongs.Columns["IdColumn"].HeaderText = Strings.Get(_currentLanguage, "ColID");
                dgvSongs.Columns["NameColumn"].HeaderText = Strings.Get(_currentLanguage, "ColNombre");
                dgvSongs.Columns["ArtistColumn"].HeaderText = Strings.Get(_currentLanguage, "ColArtista");
                dgvSongs.Columns["ReplaceColumn"].HeaderText = Strings.Get(_currentLanguage, "ReemplazarCancion");
                ((DataGridViewButtonColumn)dgvSongs.Columns["ReplaceColumn"]).Text = Strings.Get(_currentLanguage, "ReemplazarCancion");
            }

            _logger.LogInformation("Language changed to: {Language}", _currentLanguage);
        }

        private void EnableRadioControls(bool enabled)
        {
            lstRadios.Enabled = enabled;
            btnExtractBank.Enabled = enabled;
        }

        private void EnableSongControls(bool enabled)
        {
            dgvSongs.Enabled = enabled;
            btnApplyChanges.Enabled = enabled;
            btnInsertIntoGame.Enabled = enabled;
        }

        private async Task SetStatusAsync(string message, bool showProgress)
        {
            await Task.Run(() =>
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => SetStatusSync(message, showProgress)));
                }
                else
                {
                    SetStatusSync(message, showProgress);
                }
            });
        }

        private void SetStatusSync(string message, bool showProgress)
        {
            lblStatus.Text = message;
            progressBar.Visible = showProgress;
            progressBar.Style = showProgress ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
            
            Application.DoEvents(); // Allow UI to update
        }

        private async Task ShowErrorAsync(string message)
        {
            await Task.Run(() =>
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => ShowErrorSync(message)));
                }
                else
                {
                    ShowErrorSync(message);
                }
            });
        }

        private void ShowErrorSync(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            progressBar.Visible = false;
        }

        private async Task ShowInfoAsync(string message)
        {
            await Task.Run(() =>
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => ShowInfoSync(message)));
                }
                else
                {
                    ShowInfoSync(message);
                }
            });
        }

        private void ShowInfoSync(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            progressBar.Visible = false;
        }

        private void LoadConfiguration()
        {
            // Load any saved settings (window size, last language, etc.)
            _currentLanguage = _settings.UI.DefaultLanguage;
            
            if (_settings.UI.RememberWindowSize)
            {
                // Here you could load saved window size from user settings
            }

            _logger.LogInformation("Configuration loaded");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _logger.LogInformation("Disposing MainForm");
                // Dispose any managed resources
            }
            base.Dispose(disposing);
        }
    }
}