using CliHere.App.Models;

namespace CliHere.App.Services;

public sealed class CliDefinitionService
{
    private static readonly IReadOnlyList<CliDefinition> BuiltInCliDefinitions =
    [
        new() { Id = "agy", DisplayName = "AntiGravity CLI", ExecutableName = "agy", InstallUrl = "https://github.com/nicholasgasior/antigravity-cli", DocsUrl = "https://github.com/nicholasgasior/antigravity-cli" },
        new() { Id = "opencode", DisplayName = "OpenCode", ExecutableName = "opencode", InstallUrl = "https://opencode.ai", DocsUrl = "https://opencode.ai/docs" },
        new() { Id = "claude", DisplayName = "Claude Code", ExecutableName = "claude", InstallUrl = "https://docs.anthropic.com/en/docs/claude-code", DocsUrl = "https://docs.anthropic.com/en/docs/claude-code" },
        new() { Id = "codex", DisplayName = "OpenAI Codex CLI", ExecutableName = "codex", InstallUrl = "https://platform.openai.com/docs/codex", DocsUrl = "https://platform.openai.com/docs/codex" },
        new() { Id = "cursor", DisplayName = "Cursor CLI", ExecutableName = "cursor", InstallUrl = "https://cursor.sh", DocsUrl = "https://cursor.sh/docs" },
        new() { Id = "windsurf", DisplayName = "Windsurf CLI", ExecutableName = "windsurf", InstallUrl = "https://windsurf.com", DocsUrl = "https://windsurf.com/docs" },
        new() { Id = "aider", DisplayName = "Aider", ExecutableName = "aider", InstallUrl = "https://aider.chat", DocsUrl = "https://aider.chat/docs" },
        new() { Id = "continue", DisplayName = "Continue", ExecutableName = "continue", InstallUrl = "https://continue.dev", DocsUrl = "https://continue.dev/docs" },
        new() { Id = "cline", DisplayName = "Cline", ExecutableName = "cline", InstallUrl = "https://cline.bot", DocsUrl = "https://cline.bot/docs" },
    ];

    public IReadOnlyList<CliDefinition> GetAll(AppSettings? settings = null)
    {
        List<CliDefinition> result = [.. BuiltInCliDefinitions];
        if (settings is not null)
        {
            foreach (CustomCliDefinition custom in settings.CustomCliDefinitions)
            {
                if (string.IsNullOrWhiteSpace(custom.Id) ||
                    string.IsNullOrWhiteSpace(custom.DisplayName) ||
                    string.IsNullOrWhiteSpace(custom.ExecutableName))
                {
                    continue;
                }

                if (result.Any(x => string.Equals(x.Id, custom.Id, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                result.Add(custom.ToCliDefinition());
            }
        }

        return result;
    }

    public CliDefinition? GetById(string cliId, AppSettings? settings = null)
        => GetAll(settings).FirstOrDefault(cli => string.Equals(cli.Id, cliId, StringComparison.OrdinalIgnoreCase));
}
