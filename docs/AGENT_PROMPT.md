# Agent Prompt

Use this as the first prompt for an autonomous coding agent.

```text
You are an autonomous coding agent working on a new repository named `cli-here`.

Build the initial MVP foundation for a Windows utility called **CLI Here / CLI 여기서 열기**.

The purpose of the app is to let users launch AI coding CLIs such as Gemini CLI, OpenCode, Claude Code, and OpenAI Codex CLI from Windows File Explorer's right-click menu in the selected folder.

Before coding, create and read these files in order:

1. AGENTS.md
2. docs/AGENT_SPEC.md
3. docs/ARCHITECTURE.md
4. docs/ROADMAP.md
5. docs/BRANDING.md
6. docs/LOCALIZATION.md
7. docs/SECURITY.md
8. docs/RELEASE.md
9. docs/AGENT_PROMPT.md
10. .agent/tasks.md
11. .agent/progress.md
12. .agent/decisions.md
13. HISTORY.md
14. CHANGELOG.md

Project rules:

- Use C# and WPF.
- The main executable should be `CliHere.exe`.
- The repository name should be `cli-here`.
- Support Korean and English from the beginning.
- Root README.md should be English-first.
- Add README.ko.md for Korean users.
- Do not require administrator privileges for context menu registration.
- Use only HKCU registry keys for context menu integration.
- Never modify or delete registry keys not created by this app.
- Do not auto-install third-party CLIs.
- Do not add telemetry or analytics.
- Do not scan, upload, or inspect project source files.
- Add optional administrator launch mode for terminal execution.
- Use safe argument passing for process launching.

Your first task:

1. Create the initial .NET solution structure.
2. Add a WPF app project under `src/CliHere.App`.
3. Add an xUnit test project under `src/CliHere.Tests`.
4. Add the documentation skeleton files listed above.
5. Add Korean and English localization JSON files.
6. Add initial models for CLI definitions and settings.
7. Add service skeletons for CLI detection, registry context menu management, terminal launching, settings, and localization.
8. Include the `RunAsAdministrator` setting in the settings model and UI skeleton.
9. Add GitHub Actions build workflow.
10. Add README.md and README.ko.md drafts.
11. Run build and tests if possible.
12. Update `.agent/progress.md`, `.agent/tasks.md`, `HISTORY.md`, and `CHANGELOG.md`.
13. Commit the changes with a clear commit message.
14. Push the branch if remote access is configured.

Keep changes small, safe, and reviewable. Report back in Korean with changed files, validation result, branch name, commit hash, push status, and CI status if available.
```
