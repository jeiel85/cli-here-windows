# Agent Spec

## Objective

Build `CLI Here / CLI 여기서 열기`, a lightweight Windows utility that lets users launch AI coding CLIs from Windows File Explorer's right-click menu.

## Primary user

A Windows developer who frequently opens project folders and runs terminal-based AI coding tools such as Gemini CLI, OpenCode, Claude Code, and OpenAI Codex CLI.

## Core problem

The user currently has to:

1. Open PowerShell or Windows Terminal.
2. Type `cd <project-folder>`.
3. Run a CLI command such as `gemini`, `opencode`, `claude`, or `codex`.

CLI Here should reduce this to a right-click action from File Explorer.

## MVP success criteria

- A settings window opens.
- Korean and English UI are supported.
- Supported CLIs are listed.
- Installed CLIs are detected from PATH.
- Official install/documentation links are available.
- Selected context menu entries can be registered.
- Registered context menu entries can be removed.
- Folder background right-click works.
- Folder right-click works.
- PowerShell launch works.
- Windows Terminal launch works.
- Optional administrator launch works.
- UAC cancellation is handled safely.
- Settings persist across app restarts.
- GitHub Actions build/test passes.
- Release ZIP is generated.
