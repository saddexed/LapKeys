# LapKeys - Laptop Control Utility

![GitHub Release](https://img.shields.io/github/v/release/saddexed/LapKeys)
![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/saddexed/LapKeys/build.yml)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4.svg)
![Platform](https://img.shields.io/badge/platform-Windows-0078D6.svg)
![License](https://img.shields.io/github/license/saddexed/LapKeys)

A lightweight .NET WPF application for controlling laptop hardware settings including display refresh rate and brightness. Finally built a UI. This is my third time making such a project but first time actually succeeding thanks to Claude 4.5

## Features

- **Refresh Rate Control** - Switch between available display refresh rates
- **Brightness Control** - Adjust screen brightness with slider and keyboard shortcuts
- **Global Hotkeys** - Customizable keyboard shortcuts for quick access
- **Run at Startup** - Option to launch automatically with Windows
- **System Tray Integration** - Minimize to tray for background operation
- **Dark Mode** - Toggle between light and dark themes

## Requirements

### For Users

- **Windows** 10 or later (x64)
- **.NET 8 Desktop Runtime** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)

### For Development

- **Windows** 10 or later
- **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Visual Studio 2022** or **VS Code**
- 
## Installation

### Option 1: Download Release (Recommended)

1. Download the latest release from [Releases](https://github.com/saddexed/LapKeys/releases/latest)
2. Run the executable

### Option 2: Build from Source

```powershell
git clone https://github.com/saddexed/LapKeys.git
cd LapKeys
dotnet publish -c Release -o ./publish
```

The executable will be in `./publish/LapKeys.exe`

## Usage

### Refresh Rate Control

- **View Current Rate**: Displayed prominently in the Refresh Rate card
- **Switch Rates**: Click any rate button to change immediately
- **Cycle with Hotkey**: Use the keyboard shortcut (default: Ctrl+Shift+R) to cycle through selected rates
- **Customize Cycle**: Click rates in the "Rates to Cycle" section to include/exclude them
- **Visual Feedback**: On-screen overlay shows the new refresh rate

### Brightness Control

- **Adjust with Slider**: Click or drag the brightness slider
- **Quick Buttons**: Use the sun icons for +/- 10% adjustments
- **Keyboard Shortcuts**: 
  - Brightness Up: Ctrl+Shift+Up (default)
  - Brightness Down: Ctrl+Shift+Down (default)
- **Visual Feedback**: Windows-style overlay shows current brightness percentage

### Keyboard Shortcuts

1. Click the **Set** button next to any shortcut
2. Press your desired key combination (must include modifiers: Ctrl, Shift, Alt, or Win)
3. The shortcut is saved and registered immediately
4. Use the toggle switches to enable/disable shortcuts without removing them

## Project Structure

```
LapKeys/
├── ViewModels/         # MVVM ViewModels
│   ├── ViewModelBase.cs
│   └── MainViewModel.cs
├── Models/             # Data models
│   ├── DisplayMode.cs
│   ├── HotkeyBinding.cs
│   ├── RefreshRateOption.cs
│   └── AppSettings.cs
├── Services/           # Business logic services
│   ├── DisplayService.cs
│   ├── HotkeyService.cs
│   ├── BrightnessService.cs
│   ├── ThemeService.cs
│   └── SettingsService.cs
├── Views/              # Additional windows
│   ├── BrightnessOverlay.xaml
│   └── RefreshRateOverlay.xaml
├── Native/             # Windows API P/Invoke
│   └── NativeMethods.cs
├── Helpers/            # Utility classes
│   ├── RelayCommand.cs
│   ├── TrayIconManager.cs
│   ├── StartupManager.cs
│   └── Converters/
├── MainWindow.xaml     # Main application window
├── App.xaml            # Application entry and theme resources
└── LapKeys.csproj      # Project file
```

## Building from Source

### Clone and Build

```powershell
git clone https://github.com/saddexed/LapKeys.git
cd LapKeys
dotnet build
```

### Run in Development

```powershell
dotnet run
```

### Create Release Build

```powershell
# Framework-dependent (requires .NET 8 Runtime)
dotnet publish -c Release -o ./publish
```

## Issues/Limitations

- Brightness control requires WMI support
- Some hotkey combinations might be unavailable
- Refresh rate changes apply to the primary display only
- App stops tracking keys in the backgroud sometimes

## To Do

- Brightness tracking for changes not through the app
- Make overlay not steal focus 
- Better Keybind support
- Automatic updates

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Credits

- [QRes](https://github.com/Xcraft-Inc/QRes) by Xcraft for refresh rate change logic inspiration

## Contributing

Issues and pull requests are welcome! Please follow the existing code style and architecture patterns.
