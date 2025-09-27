# Forza Radio Mod Tool v2.0

A comprehensive tool for modifying radio stations in Forza games with a completely refactored architecture.

## 🎵 Features

### Core Functionality
- Extract and modify radio station music files
- Edit song metadata (title, artist)
- Support for multiple languages (EN, MX, BR, DE, FR)
- Audio format conversion (supports WAV, MP3, OGG, FLAC, AAC, M4A, WMA)
- Automatic audio resampling to 44.1kHz stereo

### New in v2.0
- **Complete architectural refactor** with separation of concerns
- **Asynchronous operations** for better UI responsiveness
- **Structured logging** with Serilog
- **Comprehensive error handling** with detailed feedback
- **Progress indicators** for long-running operations
- **Configuration management** via JSON
- **Dependency injection** for better testability
- **Modern C# patterns** and nullable reference types

## 📹 Tutorial

### 🎥 **Complete Video Tutorial**

[![ForzaRadioModTool v2.0 Tutorial](https://img.youtube.com/vi/q01OBJYeMWc/maxresdefault.jpg)](https://www.youtube.com/watch?v=q01OBJYeMWc)

**[▶️ Watch Tutorial on YouTube](https://www.youtube.com/watch?v=q01OBJYeMWc)**

Learn everything you need to know about ForzaRadioModTool v2.0:
- ✅ **Installation and setup** - Get started quickly
- ✅ **Interface walkthrough** - Navigate with confidence  
- ✅ **Replacing radio songs** - Customize your Forza experience
- ✅ **Applying changes** - Integrate mods into your game
- ✅ **Troubleshooting** - Solve common issues
- ✅ **Tips and tricks** - Pro techniques for better results

## 🏗️ Architecture

### Project Structure
```
ForzaRadioModTool/
├── Core/
│   ├── Models/          # Data models and exceptions
│   ├── Services/        # Business logic services
│   └── Interfaces/      # Service contracts
├── UI/
│   └── Forms/           # User interface
├── Configuration/       # Configuration classes
├── Common/             # Shared utilities
├── Helpers/            # Legacy helper classes
└── Tools/              # External tools (FFmpeg, FMOD)
```

### Key Services
- **RadioManager**: Main orchestrator for radio operations
- **XmlProcessor**: Handles XML parsing and manipulation
- **FileManager**: File and directory operations
- **AudioProcessor**: Audio conversion and validation
- **ConfigurationService**: Application settings management

## 📋 Requirements

### System Requirements
- Windows 10/11
- .NET 8.0 Runtime
- Forza Horizon 4/5 installed

### Dependencies
- Microsoft.Extensions.Configuration (8.0.0)
- Microsoft.Extensions.DependencyInjection (8.0.0)
- Microsoft.Extensions.Logging (8.0.0)
- Serilog (4.0.0+)

### External Tools
- FFmpeg (included in Tools/ folder)
- FMOD Bank Tools (included in Tools/FmodBankTools/)

## 🚀 Usage

1. **Setup Game Path**
   - Click "Browse..." to select your game installation directory
   - The tool will automatically validate the game files

2. **Select Language**
   - Choose your preferred language from the dropdown
   - Only languages with available game files will be shown

3. **Choose Radio Station**
   - Select a radio station from the list
   - Songs will automatically load for the selected station

4. **Modify Songs**
   - **Play**: Listen to existing songs (requires extraction first)
   - **Edit Metadata**: Click on song name or artist to edit
   - **Replace Audio**: Click "Replace song" to select a new audio file

5. **Apply Changes**
   - Click "Save Changes" to save metadata to XML files
   - Use "Extract Bank" to extract audio files for editing
   - Use "Insert into Game" to copy modified files back

## ⚙️ Configuration

The application can be configured via `appsettings.json`:

```json
{
  "Paths": {
    "MediaPath": "media",
    "AudioPath": "Audio",
    "FModBanksPath": "FMODBanks"
  },
  "Audio": {
    "DefaultVolumeDb": -13.0,
    "SampleRate": 44100
  },
  "UI": {
    "DefaultLanguage": "EN",
    "ShowTooltips": true
  },
  "Logging": {
    "LogLevel": "Information",
    "EnableFileLogging": true
  }
}
```

## 📊 Logging

Logs are written to:
- Console (during development)
- `logs/ForzaRadioModTool-{date}.log` files
- Automatic log rotation and cleanup

## 🎧 Audio Processing

### Supported Input Formats
- WAV, MP3, OGG, FLAC, AAC, M4A, WMA

### Output Format
- WAV 44.1kHz 16-bit Stereo (game requirement)
- Automatic volume normalization (-13dB default)

## 📻 Supported Radio Stations

- Horizon Pulse (R1_Tracks_CU1.assets.bank)
- Horizon Bass Arena (R2_Tracks_CU1.assets.bank)
- Horizon Block Party (R3_Tracks_CU1.assets.bank)
- Horizon XS (R4_Tracks_CU1.assets.bank)
- Hospital Records (R5_Tracks_Disk.assets.bank)
- Radio Eterna / Timeless FM (R6_Tracks_CU1.assets.bank)

## 🔧 Development

### Building from Source
1. Clone the repository
2. Ensure .NET 8.0 SDK is installed
3. Run `dotnet restore` to install dependencies
4. Run `dotnet build` to build the project
5. Run `dotnet publish` for deployment

### Testing
- Unit tests can be added to the Tests/ directory
- Services are designed for easy mocking and testing

## 🛠️ Troubleshooting

### Common Issues
1. **Game not detected**: Ensure you select the main game directory
2. **Audio conversion fails**: Check that FFmpeg.exe is in Tools/ folder
3. **FMOD extraction fails**: Verify FMOD Bank Tools are properly installed
4. **Permission errors**: Run as administrator if modifying game files

### Log Files
Check `logs/` directory for detailed error information.

## 📝 Version History

### v2.0.0
- Complete architectural refactor
- Async/await implementation
- Structured logging
- Configuration management
- Better error handling
- Modern C# patterns

### v1.0.0
- Initial release
- Basic radio modification functionality

## 📄 License

This tool is provided as-is for educational and modding purposes.

## 👥 Credits

- Original concept and implementation
- FMOD Bank Tools integration
- FFmpeg audio processing
- Community feedback and testing

---

**⚠️ Important**: Always backup your game files before using this tool. The tool automatically creates backups, but it's good practice to have your own backup as well.