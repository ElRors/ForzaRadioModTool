namespace ForzaRadioModTool.Configuration
{
    /// <summary>
    /// Application configuration settings
    /// </summary>
    public record AppSettings
    {
        public PathSettings Paths { get; init; } = new();
        public AudioSettings Audio { get; init; } = new();
        public UISettings UI { get; init; } = new();
        public LoggingSettings Logging { get; init; } = new();
    }

    public record PathSettings
    {
        public string MediaPath { get; init; } = "media";
        public string AudioPath { get; init; } = "Audio";
        public string FModBanksPath { get; init; } = "FMODBanks";
        public string ToolsPath { get; init; } = "Tools";
        public string FmodBankToolsPath { get; init; } = "Tools/FmodBankTools";
        public string BackupPath { get; init; } = "Tools/FmodBankTools/bank";
        public string WavPath { get; init; } = "Tools/FmodBankTools/wav";
        public string FsbPath { get; init; } = "Tools/FmodBankTools/fsb";
    }

    public record AudioSettings
    {
        public double DefaultVolumeDb { get; init; } = -13.0;
        public int SampleRate { get; init; } = 44100;
        public short Channels { get; init; } = 2;
        public string[] SupportedFormats { get; init; } = { "*.wav", "*.mp3", "*.ogg", "*.flac", "*.aac", "*.m4a", "*.wma" };
    }

    public record UISettings
    {
        public string DefaultLanguage { get; init; } = "EN";
        public string[] SupportedLanguages { get; init; } = { "EN", "MX", "BR", "DE", "FR" };
        public int MinWindowWidth { get; init; } = 1000;
        public int MinWindowHeight { get; init; } = 600;
        public bool ShowTooltips { get; init; } = true;
        public bool RememberWindowSize { get; init; } = true;
    }

    public record LoggingSettings
    {
        public string LogLevel { get; init; } = "Information";
        public string LogFilePath { get; init; } = "logs/ForzaRadioModTool-.log";
        public bool EnableFileLogging { get; init; } = true;
        public bool EnableConsoleLogging { get; init; } = false;
        public int MaxLogFiles { get; init; } = 10;
        public string LogFilePattern { get; init; } = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
    }
}