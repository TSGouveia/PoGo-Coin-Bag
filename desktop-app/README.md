# 🖥️ PoGo Coin Bag Desktop (Windows)

A modern, native Windows application that checks your PC's Bluetooth adapter/driver status, helps you toggle or configure Bluetooth, and launches the Nintendo Switch Postcard Receiver web console.

## ✨ Features
- 🔍 **Driver & Adapter Detection**: Verifies if your PC has an active Bluetooth adapter and driver.
- ⚡ **Radio State Status**: Instantly shows whether Bluetooth is **ON**, **OFF**, or **Disabled**.
- 🔘 **One-Click Turn On / Settings**: Toggle Bluetooth directly or open Windows Bluetooth Settings with a single click.
- 🚀 **One-Click Web Receiver**: Quickly launches the Nintendo Switch emulator Web interface in your browser to pair and receive postcards.

## 📦 Building from Source
Requirements: [.NET 10 SDK (or .NET 8+) on Windows](https://dotnet.microsoft.com/)

```bash
# Build Debug
dotnet build desktop-app/PoGoCoinBagWindows.csproj

# Publish standalone single-file .exe
dotnet publish desktop-app/PoGoCoinBagWindows.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./dist/windows
```
