using ForzaRadioModTool.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace ForzaRadioModTool.Core.Interfaces
{
    public interface IXmlProcessor
    {
        Task<OperationResult<XmlDocument>> LoadXmlAsync(string filePath);
        Task<OperationResult> SaveXmlAsync(XmlDocument document, string filePath);
        OperationResult<IEnumerable<SongInfo>> ExtractSongsFromXml(XmlDocument document, string radioName, IEnumerable<string> wavNames);
        OperationResult UpdateSongInXml(XmlDocument document, SongInfo song);
        OperationResult<IEnumerable<RadioInfo>> ExtractRadiosFromXml(XmlDocument document, string gamePath);
    }
}