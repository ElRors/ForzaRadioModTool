using System.Collections.Generic;

namespace ForzaRadioModTool.Core.Models
{
    /// <summary>
    /// Represents a radio station with its associated bank file and songs
    /// </summary>
    public record RadioInfo
    {
        public string BankFile { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public List<SongInfo> Songs { get; init; } = new();
        public bool IsAvailable { get; init; }
        public string DisplayName { get; init; } = string.Empty;

        public RadioInfo() { }

        public RadioInfo(string bankFile, string name, List<SongInfo> songs, bool isAvailable = true, string? displayName = null)
        {
            BankFile = bankFile;
            Name = name;
            Songs = songs;
            IsAvailable = isAvailable;
            DisplayName = displayName ?? name;
        }
    }
}