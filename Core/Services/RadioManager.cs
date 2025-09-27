using ForzaRadioModTool.Core.Interfaces;
using ForzaRadioModTool.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace ForzaRadioModTool.Core.Services
{
    public class RadioManager : IRadioManager
    {
        private readonly ILogger<RadioManager> _logger;
        private readonly IXmlProcessor _xmlProcessor;
        private readonly IFileManager _fileManager;
        private readonly Dictionary<string, XmlDocument> _xmlDocuments = new();

        public RadioManager(ILogger<RadioManager> logger, IXmlProcessor xmlProcessor, IFileManager fileManager)
        {
            _logger = logger;
            _xmlProcessor = xmlProcessor;
            _fileManager = fileManager;
        }

        public async Task<OperationResult<IEnumerable<RadioInfo>>> LoadRadiosAsync(string gamePath, string language)
        {
            try
            {
                _logger.LogInformation("Loading radios for game path: {GamePath}, language: {Language}", gamePath, language);

                // Validate game path
                var pathValidation = _fileManager.ValidateGamePath(gamePath);
                if (!pathValidation.IsSuccess)
                {
                    return OperationResult<IEnumerable<RadioInfo>>.Failure(pathValidation.ErrorMessage);
                }

                // Get XML path for language
                var xmlPathResult = _fileManager.GetXmlPath(gamePath, language);
                if (!xmlPathResult.IsSuccess)
                {
                    return OperationResult<IEnumerable<RadioInfo>>.Failure(xmlPathResult.ErrorMessage);
                }

                // Load XML document
                var xmlResult = await _xmlProcessor.LoadXmlAsync(xmlPathResult.Data!);
                if (!xmlResult.IsSuccess)
                {
                    return OperationResult<IEnumerable<RadioInfo>>.Failure(xmlResult.ErrorMessage);
                }

                // Cache the XML document
                _xmlDocuments[language] = xmlResult.Data!;

                // Extract radios from XML
                var radiosResult = _xmlProcessor.ExtractRadiosFromXml(xmlResult.Data!, gamePath);
                if (!radiosResult.IsSuccess)
                {
                    return OperationResult<IEnumerable<RadioInfo>>.Failure(radiosResult.ErrorMessage);
                }

                var availableRadios = radiosResult.Data!.Where(r => r.IsAvailable).ToList();
                
                _logger.LogInformation("Successfully loaded {Count} radios for language: {Language}", availableRadios.Count, language);
                return OperationResult<IEnumerable<RadioInfo>>.Success(availableRadios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading radios for game path: {GamePath}, language: {Language}", gamePath, language);
                return OperationResult<IEnumerable<RadioInfo>>.Failure($"Error loading radios: {ex.Message}", ex);
            }
        }

        public async Task<OperationResult<IEnumerable<SongInfo>>> LoadSongsAsync(string radioName, string language)
        {
            try
            {
                _logger.LogInformation("Loading songs for radio: {RadioName}, language: {Language}", radioName, language);

                if (!_xmlDocuments.TryGetValue(language, out var doc))
                {
                    return OperationResult<IEnumerable<SongInfo>>.Failure($"XML document not loaded for language: {language}");
                }

                // Get bank file for radio
                var bankFileResult = GetBankFileForRadio(radioName);
                if (!bankFileResult.IsSuccess)
                {
                    return OperationResult<IEnumerable<SongInfo>>.Failure(bankFileResult.ErrorMessage);
                }

                // Generate WAV names from fsproj
                var appFolder = AppDomain.CurrentDomain.BaseDirectory;
                var bankFolder = System.IO.Path.GetFileNameWithoutExtension(bankFileResult.Data!);
                var fsprojPath = System.IO.Path.Combine(appFolder, "Tools", "FmodBankTools", "fsb", $"{bankFileResult.Data}.fsproj");
                
                var wavNamesResult = await _fileManager.GenerateWavNamesFromFsprojAsync(fsprojPath);
                if (!wavNamesResult.IsSuccess)
                {
                    _logger.LogWarning("Could not generate WAV names from fsproj: {Error}", wavNamesResult.ErrorMessage);
                    wavNamesResult = OperationResult<IEnumerable<string>>.Success(Enumerable.Empty<string>());
                }

                // Extract songs from XML
                var songsResult = _xmlProcessor.ExtractSongsFromXml(doc, radioName, wavNamesResult.Data!);
                if (!songsResult.IsSuccess)
                {
                    return OperationResult<IEnumerable<SongInfo>>.Failure(songsResult.ErrorMessage);
                }

                _logger.LogInformation("Successfully loaded {Count} songs for radio: {RadioName}", songsResult.Data!.Count(), radioName);
                return songsResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading songs for radio: {RadioName}, language: {Language}", radioName, language);
                return OperationResult<IEnumerable<SongInfo>>.Failure($"Error loading songs: {ex.Message}", ex);
            }
        }

        public async Task<OperationResult> UpdateSongMetadataAsync(string radioName, string language, SongInfo song)
        {
            try
            {
                _logger.LogInformation("Updating song metadata: {SongId} in radio: {RadioName}", song.Id, radioName);

                if (!_xmlDocuments.TryGetValue(language, out var doc))
                {
                    return OperationResult.Failure($"XML document not loaded for language: {language}");
                }

                var updateResult = _xmlProcessor.UpdateSongInXml(doc, song);
                if (!updateResult.IsSuccess)
                {
                    return updateResult;
                }

                _logger.LogInformation("Successfully updated song metadata: {SongId}", song.Id);
                return OperationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating song metadata: {SongId}", song.Id);
                return OperationResult.Failure($"Error updating song metadata: {ex.Message}", ex);
            }
        }

        public async Task<OperationResult> SaveChangesAsync(string radioName, string language)
        {
            try
            {
                _logger.LogInformation("Saving changes for radio: {RadioName}, language: {Language}", radioName, language);

                if (!_xmlDocuments.TryGetValue(language, out var doc))
                {
                    return OperationResult.Failure($"XML document not loaded for language: {language}");
                }

                // Get XML file path
                var appFolder = AppDomain.CurrentDomain.BaseDirectory;
                var gamePath = System.IO.Directory.GetParent(System.IO.Path.Combine(appFolder, "media"))?.Parent?.FullName;
                
                if (string.IsNullOrEmpty(gamePath))
                {
                    return OperationResult.Failure("Could not determine game path");
                }

                var xmlPathResult = _fileManager.GetXmlPath(gamePath, language);
                if (!xmlPathResult.IsSuccess)
                {
                    return xmlPathResult;
                }

                // Save XML document
                var saveResult = await _xmlProcessor.SaveXmlAsync(doc, xmlPathResult.Data!);
                if (!saveResult.IsSuccess)
                {
                    return saveResult;
                }

                _logger.LogInformation("Successfully saved changes for radio: {RadioName}, language: {Language}", radioName, language);
                return OperationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes for radio: {RadioName}, language: {Language}", radioName, language);
                return OperationResult.Failure($"Error saving changes: {ex.Message}", ex);
            }
        }

        public OperationResult<string> GetBankFileForRadio(string radioName)
        {
            try
            {
                var bankFile = RadioConfiguration.GetBankFileForRadio(radioName);
                
                if (string.IsNullOrEmpty(bankFile))
                {
                    return OperationResult<string>.Failure($"Bank file not found for radio: {radioName}");
                }

                return OperationResult<string>.Success(bankFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bank file for radio: {RadioName}", radioName);
                return OperationResult<string>.Failure($"Error getting bank file: {ex.Message}", ex);
            }
        }
    }
}