# PrismDockExtension

A PowerToys Command Palette extension that surfaces your [Prism](https://github.com/severmanolescu/prism) app usage data directly in the Windows Command Palette and Dock.

## What it does

- Shows today's screen time per app, sorted by most used
- Displays app icons from Prism's icon cache
- Shows total time tracked today as the Dock label (e.g. `Today: 6h 12m`)

![Extension in Command Palette](.‎/PristDockExtension/Assets/Preview.png‎)

## Requirements

- Windows 11
- [PowerToys](https://github.com/microsoft/PowerToys) v0.90+
- Prism installed and having tracked at least one session today
- Visual Studio 2022 with **WinUI** and **Windows App SDK** workloads

## Building & deploying

1. Open `PristDockExtension.sln` in Visual Studio
2. Restore NuGet packages
3. Right-click the project → **Deploy** (not just Build — Deploy registers the MSIX package with Windows)
4. Open Command Palette (`Win + Alt + Space`)
5. Type `Reload` and select **Reload Command Palette Extension**
6. Your extension appears as **Prism Time Tracker** in the list

## Pinning to the Dock

1. Find **Prism Time Tracker** in Command Palette
2. Right-click → **Pin to Dock**
3. The Dock label shows today's total tracked time

To refresh the Dock label after pinning, run the **Reload Command Palette Extension** command again.

## Data source

The extension reads directly from Prism's SQLite database at:

```
C:\Users\<you>\AppData\Roaming\prism\data\tracker.db
```

It queries the `sessions` table joined with `apps`, filtering for today's sessions only (local midnight to midnight). Sessions with `duration = 0` (incomplete/active sessions) are excluded. Hidden apps are excluded.

## Project structure

```
PristDockExtension/
├── Assets/                          # Icons and images
├── Pages/
│   └── PristDockExtensionPage.cs    # Main list page — reads DB, builds the list
├── PristDockExtension.cs            # Extension entry point (auto-generated)
├── PristDockExtensionCommandsProvider.cs  # Registers commands, sets Dock label
├── Program.cs                       # COM server bootstrap (don't touch)
├── app.manifest
└── Package.appxmanifest
```

## Dependencies

| Package | Purpose |
|---|---|
| `Microsoft.CommandPalette.Extensions` | Command Palette SDK |
| `Microsoft.CommandPalette.Extensions.Toolkit` | Base classes and helpers |
| `Microsoft.Data.Sqlite` | SQLite access |
| `SQLitePCLRaw.bundle_e_sqlite3` | Native SQLite bundled for MSIX |
| `Shmuelie.WinRTServer` | WinRT COM server hosting |

## Limitations

- The Dock label is set once at load time and does not live-update; reload the extension to refresh it
- The DB path is currently hardcoded to `C:\Users\sever\AppData\Roaming\prism\` — change it in `PristDockExtensionPage.cs` if needed
- Requires Prism to be running and writing sessions to the DB
