# LapKeys - Laptop Control Utility

A lightweight .NET WPF application for controlling laptop hardware settings including brightness and refresh rate.

## Features

- 🎨 Modern WPF UI with MVVM architecture
- 📍 System tray integration
- 🔧 Extensible for hardware control features

## Requirements

- .NET 8.0 SDK or later
- Windows OS

## Project Structure

```
LapKeys/
├── ViewModels/         # MVVM ViewModels
│   ├── ViewModelBase.cs
│   └── MainViewModel.cs
├── Models/             # Data models
├── Views/              # Additional views (if needed)
├── Helpers/            # Utility classes
│   ├── RelayCommand.cs
│   └── TrayIconManager.cs
├── MainWindow.xaml     # Main application window
├── App.xaml            # Application entry point
└── LapKeys.csproj      # Project file
```

## Building

```powershell
dotnet build
```

## Running

```powershell
dotnet run
```

Or build and run the executable:

```powershell
dotnet build -c Release
.\bin\Release\net8.0-windows\LapKeys.exe
```

## Current Features

- **System Tray**: Minimize to tray and restore from tray icon
- **Blank UI**: Ready-to-customize main window
- **MVVM Pattern**: Proper separation of concerns for maintainability

## Planned Features

- 🔆 Display brightness control
- 🖥️ Refresh rate adjustment
- ⌨️ Keyboard shortcuts
- 💾 Settings persistence

## Development

This project follows these coding standards:

- File-scoped namespaces
- MVVM architecture pattern
- Async/await for I/O operations
- PascalCase for methods/classes
- camelCase for local variables

## License

TBD
