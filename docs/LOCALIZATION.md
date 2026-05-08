# Localization

## Supported languages

- Korean
- English

## Language modes

```text
System
Korean
English
```

## Resource files

```text
src/CliHere.App/Resources/Languages/ko.json
src/CliHere.App/Resources/Languages/en.json
```

## Rules

- Every user-facing string must have both Korean and English values.
- Context menu labels must use the selected app language.
- Do not hard-code UI strings in XAML or C#.
- CLI names and product names should remain untranslated.
- Windows technical terms may remain in English when natural.
- Korean should be natural and user-friendly.
- English should be concise and appropriate for GitHub/Reddit.

## Example keys

```json
{
  "App.Title": "CLI Here",
  "Settings.Language": "Language",
  "Settings.Terminal": "Terminal",
  "Settings.RunAsAdministrator": "Run terminal as administrator",
  "Cli.Status.Installed": "Installed",
  "Cli.Status.NotInstalled": "Not installed",
  "Cli.Action.OpenInstallPage": "Open install page",
  "ContextMenu.OpenWith": "Open {0} here",
  "Action.Apply": "Apply",
  "Action.Refresh": "Refresh",
  "Action.RemoveAll": "Remove all"
}
```

```json
{
  "App.Title": "CLI 여기서 열기",
  "Settings.Language": "언어",
  "Settings.Terminal": "터미널",
  "Settings.RunAsAdministrator": "관리자 권한으로 터미널 실행",
  "Cli.Status.Installed": "설치됨",
  "Cli.Status.NotInstalled": "설치 필요",
  "Cli.Action.OpenInstallPage": "설치 페이지 열기",
  "ContextMenu.OpenWith": "{0} 여기서 열기",
  "Action.Apply": "적용",
  "Action.Refresh": "새로고침",
  "Action.RemoveAll": "전체 제거"
}
```
