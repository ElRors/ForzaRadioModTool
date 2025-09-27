using System.Collections.Generic;
using System.Linq;

namespace ForzaRadioModTool.Core.Models
{
    /// <summary>
    /// Central configuration for radio stations and their associated bank files
    /// </summary>
    public static class RadioConfiguration
    {
        /// <summary>
        /// Mapping between bank files and radio station names
        /// </summary>
        public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> RadioBankMapping = new Dictionary<string, IReadOnlyList<string>>
        {
            { "R1_Tracks_CU1.assets.bank", new[] { "Horizon Pulse" } },
            { "R2_Tracks_CU1.assets.bank", new[] { "Horizon Bass Arena" } },
            { "R3_Tracks_CU1.assets.bank", new[] { "Horizon Block Party" } },
            { "R4_Tracks_CU1.assets.bank", new[] { "Horizon XS" } },
            { "R5_Tracks_Disk.assets.bank", new[] { "Hospital Records" } },
            { "R6_Tracks_CU1.assets.bank", new[] { "Radio Eterna", "Timeless FM" } },
        }.AsReadOnly();

        /// <summary>
        /// Get all supported radio names
        /// </summary>
        public static IEnumerable<string> GetAllRadioNames()
        {
            return RadioBankMapping.Values.SelectMany(names => names);
        }

        /// <summary>
        /// Get bank file for a specific radio name
        /// </summary>
        public static string? GetBankFileForRadio(string radioName)
        {
            return RadioBankMapping
                .Where(pair => pair.Value.Contains(radioName))
                .Select(pair => pair.Key)
                .FirstOrDefault();
        }

        /// <summary>
        /// Get radio names for a specific bank file
        /// </summary>
        public static IReadOnlyList<string> GetRadiosForBankFile(string bankFile)
        {
            return RadioBankMapping.TryGetValue(bankFile, out var radios) 
                ? radios 
                : new List<string>().AsReadOnly();
        }

        /// <summary>
        /// Check if a radio name is supported
        /// </summary>
        public static bool IsRadioSupported(string radioName)
        {
            return GetAllRadioNames().Contains(radioName);
        }

        /// <summary>
        /// Get all bank files
        /// </summary>
        public static IEnumerable<string> GetAllBankFiles()
        {
            return RadioBankMapping.Keys;
        }
        /// <summary>
        /// Gets the bank file name for a given radio station name
        /// </summary>
        /// <param name="radioName">The radio station name</param>
        /// <returns>The bank file name if found, null otherwise</returns>
        public static string? GetBankForRadio(string radioName)
        {
            return RadioBankMapping.FirstOrDefault(pair => 
                pair.Value.Any(radio => string.Equals(radio, radioName, StringComparison.OrdinalIgnoreCase))
            ).Key;
        }

        /// <summary>
        /// Gets all radio stations for a given bank file
        /// </summary>
        /// <param name="bankFile">The bank file name</param>
        /// <returns>List of radio stations for the bank file</returns>
        public static IReadOnlyList<string>? GetRadiosForBank(string bankFile)
        {
            return RadioBankMapping.TryGetValue(bankFile, out var radios) ? radios : null;
        }
    }
}