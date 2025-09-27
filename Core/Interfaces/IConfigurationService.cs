using ForzaRadioModTool.Configuration;

namespace ForzaRadioModTool.Core.Interfaces
{
    public interface IConfigurationService
    {
        AppSettings Settings { get; }
        void ReloadConfiguration();
        void SaveSettings(AppSettings settings);
        T GetSection<T>(string sectionName) where T : class, new();
    }
}