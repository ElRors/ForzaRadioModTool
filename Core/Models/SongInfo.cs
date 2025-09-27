using System.Xml;

namespace ForzaRadioModTool.Core.Models
{
    /// <summary>
    /// Represents a song with its metadata and file information
    /// </summary>
    public record SongInfo
    {
        public string Id { get; init; } = string.Empty;
        public string FileName { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Artist { get; init; } = string.Empty;
        public string WavId { get; init; } = string.Empty;
        public int SampleRate { get; init; } = 44100;
        public long SampleLength { get; init; }
        public XmlNode? XmlNode { get; init; }
        public bool HasLocalFile { get; init; }
        public string? LocalFilePath { get; init; }

        public SongInfo() { }

        public SongInfo(string id, string fileName, string name, string artist, string wavId, 
                       XmlNode? xmlNode = null, int sampleRate = 44100, long sampleLength = 0)
        {
            Id = id;
            FileName = fileName;
            Name = name;
            Artist = artist;
            WavId = wavId;
            XmlNode = xmlNode;
            SampleRate = sampleRate;
            SampleLength = sampleLength;
        }

        public SongInfo WithUpdatedMetadata(string newName, string newArtist)
        {
            return this with { Name = newName, Artist = newArtist };
        }

        public SongInfo WithLocalFile(string filePath)
        {
            return this with { LocalFilePath = filePath, HasLocalFile = true };
        }
    }
}