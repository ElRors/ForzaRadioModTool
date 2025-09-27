using System;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace ForzaRadioModTool.Helpers
{
    /// <summary>
    /// Legacy AudioUtil class for backward compatibility
    /// New implementations should use IAudioProcessor service
    /// </summary>
    public static class AudioUtil
    {
        public static bool TryGetWavInfo(string path, out int sampleRate, out short channels, out short bitsPerSample, out long sampleFrames)
        {
            sampleRate = 0; channels = 0; bitsPerSample = 0; sampleFrames = 0;
            if (!File.Exists(path)) return false;

            try
            {
                using var fs = File.OpenRead(path);
                using var br = new BinaryReader(fs);

                if (new string(br.ReadChars(4)) != "RIFF") return false;
                br.ReadUInt32();
                if (new string(br.ReadChars(4)) != "WAVE") return false;

                uint dataSize = 0;
                ushort blockAlign = 0;

                while (fs.Position < fs.Length)
                {
                    string chunkId = new string(br.ReadChars(4));
                    uint chunkSize = br.ReadUInt32();
                    long next = fs.Position + chunkSize;

                    if (chunkId == "fmt ")
                    {
                        br.ReadUInt16();                    // audioFormat
                        channels = br.ReadInt16();
                        sampleRate = br.ReadInt32();
                        br.ReadInt32();                    // byteRate
                        blockAlign = br.ReadUInt16();
                        bitsPerSample = br.ReadInt16();
                        fs.Position = next;
                    }
                    else if (chunkId == "data")
                    {
                        dataSize = chunkSize;
                        fs.Position = next;
                    }
                    else
                    {
                        fs.Position = next;
                    }
                }

                if (sampleRate <= 0 || channels <= 0 || blockAlign == 0 || dataSize == 0) return false;
                sampleFrames = dataSize / blockAlign; // frames por canal
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Convierte cualquier audio a WAV 44.1kHz estéreo usando ffmpeg.exe
        public static bool ResampleToWav441k(string inputPath, string outputPath, double volumeDb = 0)
        {
            try
            {
                string[] ffmpegPaths = new[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "ffmpeg.exe"),
                    Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.FullName ?? "", "Tools", "ffmpeg.exe"),
                    "ffmpeg"
                };

                string ffmpegExe = ffmpegPaths.FirstOrDefault(File.Exists) ?? "ffmpeg";
                string volumeArg = Math.Abs(volumeDb) > 0.01 ? $"-filter:a \"volume={volumeDb}dB\"" : "";

                var psi = new ProcessStartInfo
                {
                    FileName = ffmpegExe,
                    Arguments = $"-y -i \"{inputPath}\" -ar 44100 -ac 2 {volumeArg} \"{outputPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

                using var proc = Process.Start(psi);
                if (proc == null)
                    return false;
                
                proc.WaitForExit();
                return File.Exists(outputPath);
            }
            catch
            {
                return false;
            }
        }
    }
}