using ForzaRadioModTool.Configuration;
using ForzaRadioModTool.Core.Models;
using System;
using System.IO;
using System.Linq;

namespace ForzaRadioModTool.Common
{
    public static class ValidationHelper
    {
        public static OperationResult ValidateGamePath(string? gamePath, AppSettings settings)
        {
            if (string.IsNullOrWhiteSpace(gamePath))
                return OperationResult.Failure("Game path cannot be empty");

            if (!Directory.Exists(gamePath))
                return OperationResult.Failure("Game directory does not exist");

            var mediaPath = Path.Combine(gamePath, settings.Paths.MediaPath);
            if (!Directory.Exists(mediaPath))
                return OperationResult.Failure($"Media directory not found: {mediaPath}");

            var audioPath = Path.Combine(mediaPath, settings.Paths.AudioPath);
            if (!Directory.Exists(audioPath))
                return OperationResult.Failure($"Audio directory not found: {audioPath}");

            var fmodPath = Path.Combine(audioPath, settings.Paths.FModBanksPath);
            if (!Directory.Exists(fmodPath))
                return OperationResult.Failure($"FMODBanks directory not found: {fmodPath}");

            return OperationResult.Success();
        }

        public static OperationResult ValidateAudioFile(string? filePath, AppSettings settings)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return OperationResult.Failure("Audio file path cannot be empty");

            if (!File.Exists(filePath))
                return OperationResult.Failure("Audio file does not exist");

            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var supportedExtensions = settings.Audio.SupportedFormats
                .Select(f => f.Replace("*", "").ToLowerInvariant())
                .ToArray();

            if (!supportedExtensions.Contains(extension))
                return OperationResult.Failure($"Unsupported audio format: {extension}. Supported formats: {string.Join(", ", supportedExtensions)}");

            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length == 0)
                return OperationResult.Failure("Audio file is empty");

            if (fileInfo.Length > 100 * 1024 * 1024) // 100MB limit
                return OperationResult.Failure("Audio file is too large (max 100MB)");

            return OperationResult.Success();
        }

        public static OperationResult ValidateLanguage(string? language, AppSettings settings)
        {
            if (string.IsNullOrWhiteSpace(language))
                return OperationResult.Failure("Language cannot be empty");

            if (!settings.UI.SupportedLanguages.Contains(language))
                return OperationResult.Failure($"Unsupported language: {language}. Supported languages: {string.Join(", ", settings.UI.SupportedLanguages)}");

            return OperationResult.Success();
        }

        public static OperationResult ValidateRadioName(string? radioName)
        {
            if (string.IsNullOrWhiteSpace(radioName))
                return OperationResult.Failure("Radio name cannot be empty");

            var supportedRadios = new[]
            {
                "Horizon Pulse", "Horizon Bass Arena", "Horizon Block Party",
                "Horizon XS", "Hospital Records", "Radio Eterna", "Timeless FM"
            };

            if (!supportedRadios.Contains(radioName))
                return OperationResult.Failure($"Unsupported radio: {radioName}. Supported radios: {string.Join(", ", supportedRadios)}");

            return OperationResult.Success();
        }

        public static OperationResult ValidateSongMetadata(string? name, string? artist)
        {
            if (string.IsNullOrWhiteSpace(name))
                return OperationResult.Failure("Song name cannot be empty");

            if (string.IsNullOrWhiteSpace(artist))
                return OperationResult.Failure("Artist name cannot be empty");

            if (name.Length > 100)
                return OperationResult.Failure("Song name is too long (max 100 characters)");

            if (artist.Length > 100)
                return OperationResult.Failure("Artist name is too long (max 100 characters)");

            // Check for invalid characters
            var invalidChars = Path.GetInvalidFileNameChars().Concat(new[] { '<', '>', '&' });
            if (name.Any(c => invalidChars.Contains(c)))
                return OperationResult.Failure("Song name contains invalid characters");

            if (artist.Any(c => invalidChars.Contains(c)))
                return OperationResult.Failure("Artist name contains invalid characters");

            return OperationResult.Success();
        }

        public static OperationResult ValidateDirectory(string? path, bool createIfMissing = false)
        {
            if (string.IsNullOrWhiteSpace(path))
                return OperationResult.Failure("Directory path cannot be empty");

            try
            {
                if (!Directory.Exists(path))
                {
                    if (createIfMissing)
                    {
                        Directory.CreateDirectory(path);
                        return OperationResult.Success();
                    }
                    else
                    {
                        return OperationResult.Failure($"Directory does not exist: {path}");
                    }
                }

                // Test write access
                var testFile = Path.Combine(path, $"test_write_{Guid.NewGuid()}.tmp");
                try
                {
                    File.WriteAllText(testFile, "test");
                    File.Delete(testFile);
                }
                catch
                {
                    return OperationResult.Failure($"No write access to directory: {path}");
                }

                return OperationResult.Success();
            }
            catch (Exception ex)
            {
                return OperationResult.Failure($"Error validating directory: {ex.Message}");
            }
        }

        public static OperationResult ValidateXmlFile(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return OperationResult.Failure("XML file path cannot be empty");

            if (!File.Exists(filePath))
                return OperationResult.Failure("XML file does not exist");

            if (!Path.GetExtension(filePath).Equals(".xml", StringComparison.OrdinalIgnoreCase))
                return OperationResult.Failure("File is not an XML file");

            try
            {
                var doc = new System.Xml.XmlDocument();
                var content = File.ReadAllText(filePath);
                content = content.Replace("&", "&amp;"); // Handle unescaped ampersands
                doc.LoadXml(content);
                return OperationResult.Success();
            }
            catch (Exception ex)
            {
                return OperationResult.Failure($"Invalid XML file: {ex.Message}");
            }
        }
    }
}