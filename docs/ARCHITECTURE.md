# Architecture

## Overview

`CliHere.exe` has two modes:

1. Settings UI mode
2. Launcher mode

```text
CliHere.exe
CliHere.exe run <cliId> <folderPath>
```

Explorer context menu entries call launcher mode. Launcher mode reads settings, validates the CLI ID and folder path, then starts the configured terminal in the selected folder.

## Components

### UI Layer

- `MainWindow`
- `MainViewModel`
- CLI status list
- Language selector
- Terminal selector
- Administrator launch checkbox
- Apply/remove buttons

### Models

- `CliDefinition`
- `LocalizedText`
- `CliDetectionResult`
- `AppSettings`

### Services

- `CliDefinitionService`
- `CliDetectionService`
- `ContextMenuRegistryService`
- `TerminalLauncher`
- `SettingsService`
- `LocalizationService`

## Registry ownership

All context menu registry keys created by the app must start with:

```text
CliHere_
```

The app must only remove keys with this prefix and only under its expected registry paths.

## Registry paths

Folder background menu:

```text
HKCU\Software\Classes\Directory\Background\shell\CliHere_{cliId}
HKCU\Software\Classes\Directory\Background\shell\CliHere_{cliId}\command
```

Folder menu:

```text
HKCU\Software\Classes\Directory\shell\CliHere_{cliId}
HKCU\Software\Classes\Directory\shell\CliHere_{cliId}\command
```

## Launcher command

```text
"C:\Path\To\CliHere.exe" run gemini "%V"
"C:\Path\To\CliHere.exe" run gemini "%1"
```

## Settings path

```text
%APPDATA%\CliHere\settings.json
```

## Terminal launch

Supported terminal modes:

- Windows Terminal
- PowerShell

If Windows Terminal is selected but `wt.exe` is unavailable, fallback to PowerShell.

## Administrator mode

Administrator mode elevates only the terminal process at launch time. It does not require admin rights for setup or context menu registration.

For elevation:

- Use `UseShellExecute = true`
- Use `Verb = "runas"`
- Handle UAC cancellation
- Do not silently retry as normal user after elevation failure
