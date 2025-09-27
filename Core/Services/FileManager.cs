using ForzaRadioModTool.Core.Interfaces;
using ForzaRadioModTool.Core.Models;
using ForzaRadioModTool.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ForzaRadioModTool.Core.Services
{
    public class FileManager : IFileManager
    {
        private readonly ILogger<FileManager> _logger;
        private readonly AppSettings _settings;

        public FileManager(ILogger<FileManager> logger, AppSettings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        public OperationResult<bool> ValidateGamePath(string gamePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(gamePath))
                {
                    return OperationResult<bool>.Failure("Game path cannot be empty");
                }

                if (!Directory.Exists(gamePath))
                {
                    return OperationResult<bool>.Failure("Game path does not exist");
                }

                var mediaPath = Path.Combine(gamePath, _settings.Paths.MediaPath);
                if (!Directory.Exists(mediaPath))
                {
                    return OperationResult<bool>.Failure("Media folder not found in game directory");
                }

                var audioPath = Path.Combine(mediaPath, _settings.Paths.AudioPath);
                if (!Directory.Exists(audioPath))
                {
                    return OperationResult<bool>.Failure("Audio folder not found in game directory");
                }

                var fmodPath = Path.Combine(audioPath, _settings.Paths.FModBanksPath);
                if (!Directory.Exists(fmodPath))
                {
                    return OperationResult<bool>.Failure("FMODBanks folder not found in game directory");
                }

                _logger.LogInformation("Game path validation successful: {GamePath}", gamePath);
                return OperationResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating game path: {GamePath}", gamePath);
                return OperationResult<bool>.Failure($"Error validating game path: {ex.Message}", ex);
            }
        }

        public async Task<OperationResult<IEnumerable<string>>> GetAvailableLanguagesAsync(string gamePath)
        {
            try
            {
                var audioPath = Path.Combine(gamePath, _settings.Paths.MediaPath, _settings.Paths.AudioPath);
                var languages = new List<string>();

                foreach (var lang in _settings.UI.SupportedLanguages)
                {
                    var xmlPath = Path.Combine(audioPath, $"RadioInfo_{lang}.xml");
                    if (File.Exists(xmlPath))
                    {
                        languages.Add(lang);
                    }
                }

                _logger.LogInformation("Found {Count} available languages: {Languages}", 
                    languages.Count, string.Join(", ", languages));

                return OperationResult<IEnumerable<string>>.Success(languages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available languages from: {GamePath}", gamePath);
                return OperationResult<IEnumerable<string>>.Failure($"Error getting available languages: {ex.Message}", ex);
            }
        }

        public async Task<OperationResult> CopyBankFilesToGameAsync(string sourcePath, string gamePath)
        {
            try
            {
                var targetPath = Path.Combine(gamePath, _settings.Paths.MediaPath, _settings.Paths.AudioPath, _settings.Paths.FModBanksPath);
                int copiedCount = 0;

                foreach (var bankFile in RadioConfiguration.GetAllBankFiles())
                {
                    var sourceFile = Path.Combine(sourcePath, bankFile);
                    var targetFile = Path.Combine(targetPath, bankFile);

                    if (File.Exists(sourceFile))
                    {
                        File.Copy(sourceFile, targetFile, true);
                        copiedCount++;
                        _logger.LogInformation("Copied bank file: {BankFile}", bankFile);
                    }
                }

                _logger.LogInformation("Successfully copied {Count} bank files to game", copiedCount);
                return OperationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error copying bank files from {Source} to {Target}", sourcePath, gamePath);
                return OperationResult.Failure($"Error copying bank files: {ex.Message}", ex);
            }
        }

        public async Task<OperationResult> BackupBankFilesAsync(string gamePath, string backupPath)
        {
            try
            {
                CreateDirectoryIfNotExists(backupPath);
                var sourcePath = Path.Combine(gamePath, _settings.Paths.MediaPath, _settings.Paths.AudioPath, _settings.Paths.FModBanksPath);
                int backedUpCount = 0;

                foreach (var bankFile in RadioConfiguration.GetAllBankFiles())
                {
                    var sourceFile = Path.Combine(sourcePath, bankFile);
                    var backupFile = Path.Combine(backupPath, bankFile);

                    if (File.Exists(sourceFile))
                    {
                        File.Copy(sourceFile, backupFile, true);
                        backedUpCount++;
                        _logger.LogInformation("Backed up bank file: {BankFile}", bankFile);
                    }
                }

                _logger.LogInformation("Successfully backed up {Count} bank files", backedUpCount);
                return OperationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error backing up bank files from {GamePath} to {BackupPath}", gamePath, backupPath);
                return OperationResult.Failure($"Error backing up bank files: {ex.Message}", ex);
            }
        }

        public OperationResult<string> GetXmlPath(string gamePath, string language)
        {
            try
            {
                var xmlPath = Path.Combine(gamePath, _settings.Paths.MediaPath, _settings.Paths.AudioPath, $"RadioInfo_{language}.xml");
                
                if (!File.Exists(xmlPath))
                {
                    return OperationResult<string>.Failure($"XML file not found for language: {language}");
                }

                return OperationResult<string>.Success(xmlPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting XML path for language: {Language}", language);
                return OperationResult<string>.Failure($"Error getting XML path: {ex.Message}", ex);
            }
        }

        public async Task<OperationResult<IEnumerable<string>>> GenerateWavNamesFromFsprojAsync(string fsprojPath)
        {
            try
            {
                if (!File.Exists(fsprojPath))
                {
                    return OperationResult<IEnumerable<string>>.Success(Enumerable.Empty<string>());
                }

                var content = await File.ReadAllTextAsync(fsprojPath);
                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(content);

                var wavNames = new List<string>();
                var subsounds = doc.SelectNodes("//subsound/file");
                
                if (subsounds != null)
                {
                    foreach (System.Xml.XmlNode node in subsounds)
                    {
                        var file = node.InnerText?.Trim();
                        if (!string.IsNullOrWhiteSpace(file))
                        {
                            var wavName = Path.GetFileNameWithoutExtension(file);
                            if (!string.IsNullOrWhiteSpace(wavName))
                            {
                                wavNames.Add(wavName);
                            }
                        }
                    }
                }

                _logger.LogInformation("Generated {Count} WAV names from fsproj: {FsprojPath}", wavNames.Count, fsprojPath);
                return OperationResult<IEnumerable<string>>.Success(wavNames);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating WAV names from fsproj: {FsprojPath}", fsprojPath);
                return OperationResult<IEnumerable<string>>.Failure($"Error processing fsproj file: {ex.Message}", ex);
            }
        }

        public OperationResult CreateDirectoryIfNotExists(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    _logger.LogInformation("Created directory: {Path}", path);
                }

                return OperationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating directory: {Path}", path);
                return OperationResult.Failure($"Error creating directory: {ex.Message}", ex);
            }
        }
    }
}