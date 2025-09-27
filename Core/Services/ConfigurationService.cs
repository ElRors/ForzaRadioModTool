using ForzaRadioModTool.Configuration;
using ForzaRadioModTool.Core.Interfaces;
using ForzaRadioModTool.Core.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text.Json;

namespace ForzaRadioModTool.Core.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly string _configurationPath;
        private AppSettings _settings;
        private readonly IConfiguration _configuration;

        public AppSettings Settings => _settings;

        public ConfigurationService(string? configurationPath = null)
        {
            _configurationPath = configurationPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
            LoadConfiguration();
        }

        public void ReloadConfiguration()
        {
            LoadConfiguration();
        }

        public void SaveSettings(AppSettings settings)
        {
            try
            {
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(_configurationPath, json);
                _settings = settings;
            }
            catch (Exception ex)
            {
                throw new ConfigurationException($"Failed to save configuration: {ex.Message}", ex);
            }
        }

        public T GetSection<T>(string sectionName) where T : class, new()
        {
            var section = new T();
            _configuration.GetSection(sectionName).Bind(section);
            return section;
        }

        private void LoadConfiguration()
        {
            try
            {
                _settings = new AppSettings();
                _configuration.Bind(_settings);

                // Validate critical settings
                if (string.IsNullOrWhiteSpace(_settings.Paths.MediaPath))
                {
                    throw new ConfigurationException("MediaPath cannot be empty");
                }
            }
            catch (Exception ex) when (!(ex is ConfigurationException))
            {
                throw new ConfigurationException($"Failed to load configuration: {ex.Message}", ex);
            }
        }
    }
}