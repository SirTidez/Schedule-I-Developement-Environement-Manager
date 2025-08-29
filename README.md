# Schedule I Development Environment Manager

A desktop application built in C# to help create and manage a managed development environment for Schedule I (Schedule One on Steam).

## Features

- **Steam Integration**: Automatically detects Steam installation and library paths
- **Game Detection**: Identifies Schedule I installation within Steam libraries
- **Managed Environment Creation**: Sets up separate development environments for different branches
- **Branch Management**: Supports multiple branch types:
  - main-branch
  - beta-branch
  - alternate-branch
  - alternate-beta-branch

## Requirements

- .NET 8.0 or later
- Windows 10/11
- Steam client installed
- Schedule I game installed via Steam

## Project Structure

```
Schedule I Developement Environement Manager/
├── Models/
│   ├── SteamGameInfo.cs          # Steam game information model
│   └── DevEnvironmentConfig.cs   # Development environment configuration
├── Services/
│   └── SteamService.cs           # Steam integration service
├── MainForm.cs                   # Main application form
├── MainForm.Designer.cs          # Windows Forms designer file
├── Program.cs                    # Application entry point
├── Schedule I Developement Environement Manager.csproj  # C# project file
└── README.md                     # This file
```

## How It Works

1. **Steam Detection**: The application automatically scans for Steam installation and library paths
2. **Game Location**: Users can browse and select the Schedule I game installation folder
3. **Environment Setup**: Users specify where they want the managed environment created
4. **Branch Selection**: Users select which branches they want to create
5. **Environment Creation**: The application copies game files to separate branch directories

## Configuration

### Steam ID
The application currently uses a placeholder Steam ID for Schedule I. You'll need to update the `ScheduleISteamId` constant in `SteamService.cs` with the actual Steam App ID for Schedule I.

### Default Paths
- Steam Library: `C:\Program Files (x86)\Steam\steamapps` (placeholder)
- Game Installation: Auto-detected from Steam library
- Managed Environment: User-selected location

## Building and Running

1. Open the solution in Visual Studio 2022 or later
2. Restore NuGet packages
3. Build the solution
4. Run the application

## Dependencies

- Microsoft.Extensions.Logging
- Microsoft.Extensions.Logging.Console
- Newtonsoft.Json

## Notes

- The application requires administrative privileges to copy game files
- Large game installations may take significant time to copy
- Ensure sufficient disk space for multiple branch copies
- The application creates exact copies of the game files for each branch

## Future Enhancements

- Steam Workshop integration
- Mod management per branch
- Configuration file support
- Update checking and management
- Branch-specific settings and configurations
