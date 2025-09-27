using ForzaRadioModTool.Core.Interfaces;
using ForzaRadioModTool.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace ForzaRadioModTool.Core.Services
{
    public class XmlProcessor : IXmlProcessor
    {
        private readonly ILogger<XmlProcessor> _logger;

        public XmlProcessor(ILogger<XmlProcessor> logger)
        {
            _logger = logger;
        }

        public async Task<OperationResult<XmlDocument>> LoadXmlAsync(string filePath)
        {
            try
            {
                _logger.LogInformation("Loading XML file: {FilePath}", filePath);
                
                var doc = new XmlDocument();
                var xmlContent = await System.IO.File.ReadAllTextAsync(filePath);
                
                // Escape ampersands for XML parsing
                xmlContent = xmlContent.Replace("&", "&amp;");
                doc.LoadXml(xmlContent);

                _logger.LogInformation("Successfully loaded XML file: {FilePath}", filePath);
                return OperationResult<XmlDocument>.Success(doc);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading XML file: {FilePath}", filePath);
                return OperationResult<XmlDocument>.Failure($"Error loading XML file: {ex.Message}", ex);
            }
        }

        public async Task<OperationResult> SaveXmlAsync(XmlDocument document, string filePath)
        {
            try
            {
                _logger.LogInformation("Saving XML file: {FilePath}", filePath);
                
                document.Save(filePath);
                
                // Unescape ampersands after saving
                var xmlContent = await System.IO.File.ReadAllTextAsync(filePath);
                xmlContent = xmlContent.Replace("&amp;", "&");
                await System.IO.File.WriteAllTextAsync(filePath, xmlContent);

                _logger.LogInformation("Successfully saved XML file: {FilePath}", filePath);
                return OperationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving XML file: {FilePath}", filePath);
                return OperationResult.Failure($"Error saving XML file: {ex.Message}", ex);
            }
        }

        public OperationResult<IEnumerable<SongInfo>> ExtractSongsFromXml(XmlDocument document, string radioName, IEnumerable<string> wavNames)
        {
            try
            {
                _logger.LogInformation("Extracting songs from XML for radio: {RadioName}", radioName);

                var songs = new List<SongInfo>();
                var selectedRadio = document.SelectSingleNode($"/Radio/RadioStations/RadioStation[@Name='{radioName}']");

                if (selectedRadio == null)
                {
                    _logger.LogWarning("Radio station not found in XML: {RadioName}", radioName);
                    return OperationResult<IEnumerable<SongInfo>>.Success(songs);
                }

                var samples = selectedRadio.SelectNodes("SampleList/*");
                if (samples == null)
                {
                    _logger.LogWarning("No samples found for radio: {RadioName}", radioName);
                    return OperationResult<IEnumerable<SongInfo>>.Success(songs);
                }

                var wavNamesList = wavNames.ToList();

                for (int i = 0; i < samples.Count && i < wavNamesList.Count; i++)
                {
                    var sample = samples[i];
                    if (sample == null) continue;

                    var song = ExtractSongFromXmlNode(sample, wavNamesList[i]);
                    if (song != null)
                    {
                        songs.Add(song);
                    }
                }

                _logger.LogInformation("Extracted {Count} songs from XML for radio: {RadioName}", songs.Count, radioName);
                return OperationResult<IEnumerable<SongInfo>>.Success(songs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting songs from XML for radio: {RadioName}", radioName);
                return OperationResult<IEnumerable<SongInfo>>.Failure($"Error extracting songs: {ex.Message}", ex);
            }
        }

        public OperationResult UpdateSongInXml(XmlDocument document, SongInfo song)
        {
            try
            {
                _logger.LogInformation("Updating song in XML: {SongId}", song.Id);

                if (song.XmlNode?.LocalName == "Sample")
                {
                    var xmlNode = song.XmlNode;
                    
                    // Update display name
                    if (xmlNode.Attributes?["DisplayName"] != null)
                    {
                        xmlNode.Attributes["DisplayName"]!.Value = song.Name;
                    }
                    
                    // Update artist
                    if (xmlNode.Attributes?["Artist"] != null)
                    {
                        xmlNode.Attributes["Artist"]!.Value = song.Artist;
                    }

                    // Update sample rate if provided
                    if (song.SampleRate > 0)
                    {
                        var rateAttr = xmlNode.Attributes?["SampleRate"];
                        if (rateAttr == null)
                        {
                            rateAttr = xmlNode.OwnerDocument!.CreateAttribute("SampleRate");
                            xmlNode.Attributes!.Append(rateAttr);
                        }
                        rateAttr.Value = song.SampleRate.ToString();
                    }

                    // Update sample length if provided
                    if (song.SampleLength > 0)
                    {
                        var lenAttr = xmlNode.Attributes?["SampleLength"];
                        if (lenAttr == null)
                        {
                            lenAttr = xmlNode.OwnerDocument!.CreateAttribute("SampleLength");
                            xmlNode.Attributes!.Append(lenAttr);
                        }
                        lenAttr.Value = song.SampleLength.ToString();

                        // Update End marker if exists
                        var endMarker = xmlNode.SelectSingleNode("Marker[@Name='End']") as XmlElement;
                        if (endMarker != null)
                        {
                            endMarker.SetAttribute("Position", Math.Max(0, song.SampleLength - 1).ToString());
                        }
                    }
                }

                _logger.LogInformation("Successfully updated song in XML: {SongId}", song.Id);
                return OperationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating song in XML: {SongId}", song.Id);
                return OperationResult.Failure($"Error updating song: {ex.Message}", ex);
            }
        }

        public OperationResult<IEnumerable<RadioInfo>> ExtractRadiosFromXml(XmlDocument document, string gamePath)
        {
            try
            {
                _logger.LogInformation("Extracting radios from XML");

                var radios = new List<RadioInfo>();
                var radioStations = document.SelectNodes("/Radio/RadioStations/RadioStation");

                if (radioStations == null)
                {
                    _logger.LogWarning("No radio stations found in XML");
                    return OperationResult<IEnumerable<RadioInfo>>.Success(radios);
                }

                foreach (var pair in RadioConfiguration.RadioBankMapping)
                {
                    var bankFile = pair.Key;
                    var bankPath = System.IO.Path.Combine(gamePath, "media", "Audio", "FMODBanks", bankFile);
                    var isAvailable = System.IO.File.Exists(bankPath);

                    foreach (var radioName in pair.Value)
                    {
                        var radioNode = document.SelectSingleNode($"/Radio/RadioStations/RadioStation[@Name='{radioName}']");
                        if (radioNode != null)
                        {
                            var radio = new RadioInfo(bankFile, radioName, new List<SongInfo>(), isAvailable);
                            radios.Add(radio);
                        }
                    }
                }

                _logger.LogInformation("Extracted {Count} radios from XML", radios.Count);
                return OperationResult<IEnumerable<RadioInfo>>.Success(radios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting radios from XML");
                return OperationResult<IEnumerable<RadioInfo>>.Failure($"Error extracting radios: {ex.Message}", ex);
            }
        }

        private SongInfo? ExtractSongFromXmlNode(XmlNode sample, string wavId)
        {
            try
            {
                string fileName = "";
                string name = "";
                string artist = "";
                int sampleRate = 44100;
                long sampleLength = 0;

                if (sample.LocalName == "Sample")
                {
                    fileName = sample.Attributes?["SoundName"]?.Value ?? "";
                    name = sample.Attributes?["DisplayName"]?.Value ?? "";
                    artist = sample.Attributes?["Artist"]?.Value ?? "";
                    
                    if (int.TryParse(sample.Attributes?["SampleRate"]?.Value, out var rate))
                        sampleRate = rate;
                    
                    if (long.TryParse(sample.Attributes?["SampleLength"]?.Value, out var length))
                        sampleLength = length;
                }
                else if (sample.LocalName == "Entry")
                {
                    fileName = sample.Attributes?["Name"]?.Value ?? "";
                    var entryName = sample.Attributes?["Name"]?.Value ?? "";
                    var parts = entryName.Split('_');
                    if (parts.Length > 2)
                    {
                        name = string.Join(" ", parts.Skip(2));
                        artist = "Various Artists";
                    }
                }

                return new SongInfo(wavId, fileName, name, artist, wavId, sample, sampleRate, sampleLength);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error extracting song from XML node");
                return null;
            }
        }
    }
}