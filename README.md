# LapKeys - Laptop Control Utility

A lightweight .NET WPF application for controlling laptop hardware settings including brightness and refresh rate.

## Features

- 🖥️ **Refresh Rate Cycling** - Cycle through available display refresh rates
- ⌨️ **Global Hotkeys** - Win+F1 (default) to cycle refresh rates from anywhere
- 🎨 Modern WPF UI with MVVM architecture
- 📍 System tray integration
- 🔧 Extensible for additional hardware control features

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
│   ├── DisplayMode.cs
│   └── HotkeyBinding.cs
├── Services/           # Business logic services
│   ├── DisplayService.cs
│   └── HotkeyService.cs
├── Native/             # Windows API interop
│   └── NativeMethods.cs
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

## Usage

### Refresh Rate Control

- **View Current Rate**: The main window shows your current refresh rate
- **Click Rate Buttons**: Click any available rate button to switch immediately
- **Cycle Button**: Click "Cycle Refresh Rate" to move to the next available rate
- **Global Hotkey**: Press `Win+F1` (default) anywhere to cycle refresh rates

### Hotkey Configuration

1. Click the "Change" button next to the hotkey display
2. Press your desired key combination (must include a modifier like Win, Ctrl, Alt, or Shift)
3. The new hotkey is registered immediately

### System Tray

- Minimize the window to hide it to the system tray
- Double-click the tray icon to restore
- Right-click for menu options

## Current Features

- ✅ **Display Refresh Rate Control** - Uses Windows API (similar to QRes)
- ✅ **Global Hotkeys** - Customizable keyboard shortcuts
- ✅ **System Tray** - Minimize to tray and restore from tray icon
- ✅ **MVVM Pattern** - Proper separation of concerns for maintainability

## Planned Features

- 🔆 Display brightness control
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
