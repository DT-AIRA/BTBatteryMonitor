# BTBatteryMonitor

[English](README_en.md) | [日本語](README.md)

A lightweight desktop battery monitor widget for Windows 10 and 11.  
Displays the battery levels of connected Bluetooth devices directly on your desktop.

<p align="center">
  <img src="docs/images/screenshot.png" alt="BTBatteryMonitor Widget Preview" width="300" />
</p>

---

## Features

- **Battery Monitoring**: View battery levels of connected Bluetooth devices (headphones, mice, keyboards, etc.).
- **Event-Driven Updates**: Automatically updates via Windows device change events (`WM_DEVICECHANGE`) with low CPU usage.
- **Draggable & Position Saving**: Place the widget anywhere on your desktop; position is remembered across restarts.
- **Customization**: Adjust widget background color (presets or custom palette) and opacity via context menu.
- **Startup Support**: Easily enable or disable launching on Windows startup.

---

## Requirements

- **OS**: Windows 10 (Build 19041 or higher) / Windows 11 (x64)
- **Bluetooth**: Bluetooth 4.0 or higher

---

## Installation

### Installer
1. Download `BTBatteryMonitor_Setup.exe` from [Releases](https://github.com/DT-AIRA/BTBatteryMonitor/releases).
2. Run the installer and follow the on-screen instructions (no administrator privileges required).

### Portable
1. Download `BTBatteryMonitor.exe` from [Releases](https://github.com/DT-AIRA/BTBatteryMonitor/releases).
2. Place it in any folder and run it.

---

## Usage

- **Move Widget**: Click and drag the widget anywhere on your desktop.
- **Context Menu (Right-Click)**:
  - **Refresh**: Manually refresh device battery levels.
  - **Widget Color**: Change background color (5 preset colors or pick from a color dialog).
  - **Widget Opacity**: Adjust background opacity (100% / 85% / 65% / 45% / 25%).
  - **Always on Top**: Toggle always-on-top window display.
  - **Launch at Startup**: Toggle launching at Windows startup.
  - **Exit**: Quit the application.

---

## Building from Source

```bat
setup_env.bat   # Set up local dev environment (.NET 8 SDK)
build.bat       # Build project
run.bat         # Run widget
publish.bat     # Publish single-file EXE and create installer
```

---

## Tech Stack

- **Framework**: WPF (.NET 8 Windows Desktop / x64)
- **APIs**: Win32 SetupAPI / WinRT (`Windows.Devices.Bluetooth`, `Windows.Devices.Enumeration`)
- **Installer**: Inno Setup 6

---

## License

Copyright (c) 2026 DT-AIRA  
This project is licensed under the [Apache License, Version 2.0](LICENSE).
