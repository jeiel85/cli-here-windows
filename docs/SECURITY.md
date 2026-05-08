# Security

## Principles

- No telemetry.
- No analytics.
- No background network requests.
- No project file scanning.
- No project file upload.
- No automatic third-party CLI installation in MVP.
- No administrator requirement for normal usage.
- Registry writes are limited to HKCU.
- The app deletes only registry keys it created.

## Context menu safety

Registry command values must call:

```text
CliHere.exe run <cliId> <folderPath>
```

Do not embed arbitrary CLI commands directly in registry values.

## Administrator launch

Administrator mode must:

- Be optional.
- Elevate only the terminal process.
- Use Windows UAC.
- Handle UAC cancellation gracefully.
- Never silently retry as normal user after elevation failure.

## Registry safety

- Use only `HKCU\Software\Classes`.
- Use `CliHere_` key prefix.
- Delete only known `CliHere_` keys under expected paths.

## Process launching

- Prefer `ProcessStartInfo.ArgumentList` for normal launch.
- For elevated launch, use `UseShellExecute = true` and `Verb = "runas"`.
- Isolate quoting and escaping logic.
- Test paths with spaces, Korean characters, and special characters.
