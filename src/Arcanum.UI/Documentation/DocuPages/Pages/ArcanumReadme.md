---
id: "debug.hello" # The id of the feature the documentaiton is about. Ids can be browsed in debug mode.
title: "Some search and rescue" # Title of the page.
summary: "Be ambitious" # Short summary of what this page is about.
links: ["security.md", "profile.md"] # Links to e.g. the wiki page for this page.
searchKeywords: ["preferences", "settings", "account", "themes", "privacy"] # Any number of keywords that are associated with this page for search purposes.
category: "Editor" # Possible options: SpecializedEditor, Editor, Debug, Configuration, EditorMap
level: "System" # Possible options: System, Module, Panel, Widget, Action
scale: "Full" # Possible options: Compact, Standard, Major, Full
location: "Right" # Possible options: Center, Top, TopRight, Right, BottomRight, Bottom, BottomLeft, Left, TopLeft
status: "Stable" # Possible options: Stable, Beta, Experimental, Legacy
iconPath: "map.png" # OPTIONAL Icon path realtive to Documentation/Icons/
associatedScopes: ["Global", "Editor"] # Options can be seen dev tools.
introducedIn: "1.0.7.2" # Version the feature was introduced in.
---

# The Arcanum Project

The **Arcanum Project** is a bundle of tools to ease the modding of Europa Universalis 5 (EU5).
The tools are mainly being developed by [Minnator](https://github.com/Minnator)
and [MelonCoaster](https://github.com/mel-co) with the help
of [CzerstfyChlep](https://github.com/CzerstfyChlep), [zulacecek](https://github.com/zulacecek) and other contributors.

This tool is in active development and will continue to become more feature-rich and polished.

## Documentation

Our documentation is a work in progress. You can access the currently available
parts [here](https://the-arcanum-project.github.io/Arcanum/user/about-arcanum.html)

## Overview

- Desktop application for Windows built with .NET and WPF.
- Provides specialized editors (NUI) and an interactive map to edit a wide range of EU5 objects.
- Features include an undo/redo history, hot-reloading of game/mod files, error detection with precise locations, and
  editing support for 40+ game object types.

## Technology Stack

- Language: C#
- Runtime/Target framework: `net10.0-windows`
- UI framework: WPF
- Build system & package manager: `dotnet` CLI / MSBuild, NuGet
- Test framework: NUnit (with `NUnit3TestAdapter` and `Microsoft.NET.Test.Sdk`)

## Requirements

- Windows 10/11 (x64)
- .NET SDK 10, only if building from source
- Europa Universalis 5 installed (for real data paths used by the app)

## Quick Setup (Using a Release)

1. Back up your mod files.
2. Download the latest release from GitHub.
3. Run the `script_docs` command in EU5
4. Start Arcanum and enter the required mod and vanilla paths in the main menu.
5. Launch the current config and start modding.

## Build From Source

Clone the repository and use the according .NET SDK.

```powershell
git clone https://github.com/The-Arcanum-Project/Arcanum
cd Arcanum
dotnet restore
dotnet build Arcanum.sln -c Release
```

### Run the App (from source)

Use the app project in `src/Arcanum.App` (WPF WinExe).

```powershell
dotnet run -c Debug --project src\Arcanum.App\Arcanum.App.csproj
```

This produces a Windows desktop application named `Arcanum [ReleaseName]` (see `AssemblyName` in the csproj).

## Editing – Quick Guide

- Select any object via search or the map using different selection modes.
- Once an object is selected it is loaded into the NUI (specialized editor).
- Edit any values you need in NUI.
- Before hitting `Ctrl+S` to save all changes, review the save settings to ensure they match your preferences.
- Save your changes.

## Implemented Features

- Smart history tree with undo/redo for any action taken
- Interactive map with map modes
- Detailed error detection pinpointing file, line, and character
- Hot reloading support for mod and game files
- Support for more than 40 different game objects

## Current Limitations

- Objects with effects and triggers are not yet editable

## Roadmap / Future Features

- Map editor
- Intelligent map design aides
- Automatic error correction
- Plugin support
- User-defined map modes
- Map exporting
- Heightmap and normals on the interactive map
- Editing support for all objects

## Tests

NUnit-based test project is located at `src/dev/UnitTests`.

Run the tests:

```powershell
dotnet test src\dev\UnitTests\UnitTests.csproj -c Release
```

## Project Structure

Top-level directories of interest:

- `src/Arcanum.App` — WPF application entry point (`WinExe`, `net10.0-windows`).
- `src/Arcanum.UI` — UI components, styles, controls, and NUI (WPF library).
- `src/Arcanum.Core` — Core logic, parsing, map systems, registries, etc.
- `src/Arcanum.PluginHost` — Host for plugins (referenced by Core/UI).
- `src/Arcanum.SDK` — SDK surface for extensions and external integrations.
- `src/dev/UnitTests` — NUnit-based tests targeting Core/SDK.
- `src/src_gen/*` — Source generators, analyzers, and code fixers used at build time.
- `Common`, `Nexus.Core`, `DiagnosticArgsAnalyzer` — shared libs/analyzers referenced by projects.
- `docs` — user documentation site sources (WIP).

## License

This project is licensed. See the [LICENSE](https://github.com/The-Arcanum-Project/Arcanum/blob/main/LICENSE) file in
the repository root for details.

## Input and Contribution

We are currently not actively looking for contributors, but we are open to ideas and suggestions.
If you have any suggestions, questions, or feedback, feel free to reach out on the
official [Discord server](https://discord.gg/CXFGsEgugn)

