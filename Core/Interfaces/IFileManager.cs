using ForzaRadioModTool.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ForzaRadioModTool.Core.Interfaces
{
    public interface IFileManager
    {
        OperationResult<bool> ValidateGamePath(string gamePath);
        Task<OperationResult<IEnumerable<string>>> GetAvailableLanguagesAsync(string gamePath);
        Task<OperationResult> CopyBankFilesToGameAsync(string sourcePath, string gamePath);
        Task<OperationResult> BackupBankFilesAsync(string gamePath, string backupPath);
        OperationResult<string> GetXmlPath(string gamePath, string language);
        Task<OperationResult<IEnumerable<string>>> GenerateWavNamesFromFsprojAsync(string fsprojPath);
        OperationResult CreateDirectoryIfNotExists(string path);
    }
}