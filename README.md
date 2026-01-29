# AotuPaintFramework

Material Paint Tool for Revit 2025

## Overview
This is a WPF-based material painting tool for Autodesk Revit 2025, built with Nice3Point.Revit.Toolkit framework.

## Project Structure

```
AotuPaintFramework/
├── Source/
│   ├── AotuPaintFramework/          # Main plugin project
│   │   ├── Commands/                # Revit command implementations
│   │   ├── ViewModels/              # MVVM view models
│   │   ├── Views/                   # WPF views
│   │   ├── Models/                  # Data models
│   │   ├── Utils/                   # Utility classes
│   │   └── AotuPaintFramework.csproj
│   └── Build/                       # Nuke build project
│       ├── Build.cs
│       └── Build.csproj
└── AotuPaintFramework.sln           # Solution file
```

## Features (Coming Soon)
- Select elements in 3D view
- Category-based material mapping
- Paint side faces, bottom faces, and interface faces
- Remove paint functionality
- Save/Load mapping configurations

## Requirements
- Autodesk Revit 2025
- .NET 8 SDK
- Visual Studio 2022 or JetBrains Rider

## Building

### Using .NET CLI
```bash
dotnet restore
dotnet build
```

### Using Nuke Build
```bash
dotnet run --project Source/Build/Build.csproj
```

## Dependencies

- Nice3point.Revit.Toolkit 2025.0.3
- Nice3point.Revit.Extensions 2025.1.0
- MaterialDesignThemes 5.1.0
- MaterialDesignColors 3.1.0

## Development

The project uses:
- **Nice3Point.Revit.Toolkit** for simplified Revit API integration
- **MaterialDesign** for modern WPF UI components
- **Nuke** for build automation
- **.NET 8** with C# latest features enabled

## Status
🚧 Under development

## License
MIT
