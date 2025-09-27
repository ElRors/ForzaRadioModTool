using ForzaRadioModTool.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ForzaRadioModTool.Core.Interfaces
{
    public interface IRadioManager
    {
        Task<OperationResult<IEnumerable<RadioInfo>>> LoadRadiosAsync(string gamePath, string language);
        Task<OperationResult<IEnumerable<SongInfo>>> LoadSongsAsync(string radioName, string language);
        Task<OperationResult> UpdateSongMetadataAsync(string radioName, string language, SongInfo song);
        Task<OperationResult> SaveChangesAsync(string radioName, string language);
        OperationResult<string> GetBankFileForRadio(string radioName);
    }
}