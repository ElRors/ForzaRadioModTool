using ForzaRadioModTool.Configuration;
using ForzaRadioModTool.Core.Interfaces;
using ForzaRadioModTool.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ForzaRadioModTool.Core.Services
{
    public class AudioProcessor : IAudioProcessor
    {
        private readonly ILogger<AudioProcessor> _logger;
        private readonly AppSettings _settings;

        public AudioProcessor(ILogger<AudioProcessor> logger, AppSettings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        public async Task<OperationResult> ReplaceAudioAsync(string sourceFile, string targetFile, double volumeDb = -13.0)
        {
            try
            {
                _logger.LogInformation("Converting audio from {Source} to {Target}", sourceFile, targetFile);

                if (!File.Exists(sourceFile))
                {
                    return OperationResult.Failure("Source audio file does not exist");
                }

                var ffmpegExe = GetFFmpegPath();
                if (string.IsNullOrEmpty(ffmpegExe))
                {
                    return OperationResult.Failure("FFmpeg not found");
                }

                // Ensure target directory exists
                var targetDir = Path.GetDirectoryName(targetFile);
                if (!string.IsNullOrEmpty(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                var volumeArg = Math.Abs(volumeDb) > 0.01 ? $"-filter:a \"volume={volumeDb}dB\"" : "";
                var arguments = $"-y -i \"{sourceFile}\" -ar {_settings.Audio.SampleRate} -ac {_settings.Audio.Channels} {volumeArg} \"{targetFile}\"";

                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = ffmpegExe,
                        Arguments = arguments,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    }
                };

                process.Start();
                var stderr = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    _logger.LogError("FFmpeg failed with exit code {ExitCode}: {Error}", process.ExitCode, stderr);
                    return OperationResult.Failure($"Audio conversion failed: {stderr}");
                }

                if (!File.Exists(targetFile))
                {
                    return OperationResult.Failure("Converted file was not created");
                }

                _logger.LogInformation("Successfully converted audio: {TargetFile}", targetFile);
                return OperationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting audio from {Source} to {Target}", sourceFile, targetFile);
                return OperationResult.Failure($"Error converting audio: {ex.Message}", ex);
            }
        }

        public OperationResult<(int sampleRate, short channels, short bitsPerSample, long sampleFrames)> GetAudioInfo(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return OperationResult<(int, short, short, long)>.Failure("Audio file does not exist");
                }

                // Use existing AudioUtil method for backward compatibility and avoid code duplication
                if (ForzaRadioModTool.Helpers.AudioUtil.TryGetWavInfo(filePath, out var sampleRate, out var channels, out var bitsPerSample, out var sampleFrames))
                {
                    _logger.LogInformation("Audio info - Rate: {Rate}, Channels: {Channels}, Bits: {Bits}, Frames: {Frames}", 
                        sampleRate, channels, bitsPerSample, sampleFrames);
                    return OperationResult<(int, short, short, long)>.Success((sampleRate, channels, bitsPerSample, sampleFrames));
                }

                return OperationResult<(int, short, short, long)>.Failure("Invalid WAV file data");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audio info for file: {FilePath}", filePath);
                return OperationResult<(int, short, short, long)>.Failure($"Error reading audio info: {ex.Message}", ex);
            }
        }

        public async Task<OperationResult> ExtractBankAsync(string bankFile, string outputPath)
        {
            try
            {
                _logger.LogInformation("Extracting bank file: {BankFile} to {OutputPath}", bankFile, outputPath);

                var fmodToolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _settings.Paths.FmodBankToolsPath, "Fmod Bank Tools.exe");
                if (!File.Exists(fmodToolPath))
                {
                    return OperationResult.Failure("FMOD Bank Tools not found");
                }

                if (!File.Exists(bankFile))
                {
                    return OperationResult.Failure("Bank file does not exist");
                }

                Directory.CreateDirectory(outputPath);

                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = fmodToolPath,
                        Arguments = $"-format vorbis -quality 90 -o \"{outputPath}\" \"{bankFile}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    var error = await process.StandardError.ReadToEndAsync();
                    _logger.LogError("FMOD extraction failed with exit code {ExitCode}: {Error}", process.ExitCode, error);
                    return OperationResult.Failure($"Bank extraction failed: {error}");
                }

                _logger.LogInformation("Successfully extracted bank file: {BankFile}", bankFile);
                return OperationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting bank file: {BankFile}", bankFile);
                return OperationResult.Failure($"Error extracting bank: {ex.Message}", ex);
            }
        }

        public async Task<OperationResult> BuildBankAsync(string projectPath)
        {
            try
            {
                _logger.LogInformation("Building bank from project: {ProjectPath}", projectPath);

                var fmodToolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _settings.Paths.FmodBankToolsPath, "Fmod Bank Tools.exe");
                if (!File.Exists(fmodToolPath))
                {
                    return OperationResult.Failure("FMOD Bank Tools not found");
                }

                if (!File.Exists(projectPath))
                {
                    return OperationResult.Failure("Project file does not exist");
                }

                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = fmodToolPath,
                        Arguments = $"\"{projectPath}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    var error = await process.StandardError.ReadToEndAsync();
                    _logger.LogError("FMOD build failed with exit code {ExitCode}: {Error}", process.ExitCode, error);
                    return OperationResult.Failure($"Bank build failed: {error}");
                }

                _logger.LogInformation("Successfully built bank from project: {ProjectPath}", projectPath);
                return OperationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building bank from project: {ProjectPath}", projectPath);
                return OperationResult.Failure($"Error building bank: {ex.Message}", ex);
            }
        }

        public OperationResult ValidateAudioFile(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    return OperationResult.Failure("File path cannot be empty");
                }

                if (!File.Exists(filePath))
                {
                    return OperationResult.Failure("Audio file does not exist");
                }

                var extension = Path.GetExtension(filePath).ToLowerInvariant();
                var supportedExtensions = _settings.Audio.SupportedFormats
                    .Select(f => f.Replace("*", ""))
                    .ToArray();

                if (!supportedExtensions.Contains(extension))
                {
                    return OperationResult.Failure($"Unsupported audio format: {extension}");
                }

                // Additional validation for WAV files
                if (extension == ".wav")
                {
                    var audioInfoResult = GetAudioInfo(filePath);
                    if (!audioInfoResult.IsSuccess)
                    {
                        return OperationResult.Failure($"Invalid WAV file: {audioInfoResult.ErrorMessage}");
                    }
                }

                _logger.LogInformation("Audio file validation successful: {FilePath}", filePath);
                return OperationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating audio file: {FilePath}", filePath);
                return OperationResult.Failure($"Error validating audio file: {ex.Message}", ex);
            }
        }

        private string? GetFFmpegPath()
        {
            var ffmpegPaths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _settings.Paths.ToolsPath, "ffmpeg.exe"),
                Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.FullName ?? "", _settings.Paths.ToolsPath, "ffmpeg.exe"),
                "ffmpeg"
            };

            return ffmpegPaths.FirstOrDefault(File.Exists) ?? 
                   ffmpegPaths.LastOrDefault(); // Return "ffmpeg" as fallback for PATH lookup
        }
    }
}