using ForzaRadioModTool.Core.Models;
using System.Threading.Tasks;

namespace ForzaRadioModTool.Core.Interfaces
{
    public interface IAudioProcessor
    {
        Task<OperationResult> ReplaceAudioAsync(string sourceFile, string targetFile, double volumeDb = -13.0);
        OperationResult<(int sampleRate, short channels, short bitsPerSample, long sampleFrames)> GetAudioInfo(string filePath);
        Task<OperationResult> ExtractBankAsync(string bankFile, string outputPath);
        Task<OperationResult> BuildBankAsync(string projectPath);
        OperationResult ValidateAudioFile(string filePath);
    }
}