using CliHere.App.Models;

namespace CliHere.App.Services;

public sealed class CliDefinitionService
{
    private static readonly IReadOnlyList<CliDefinition> BuiltInCliDefinitions =
    [
        new() { Id = "gemini", DisplayName = "Gemini CLI", ExecutableName = "gemini", InstallUrl = "https://github.com/google-gemini/gemini-cli", DocsUrl = "https://github.com/google-gemini/gemini-cli" },
        new() { Id = "opencode", DisplayName = "OpenCode", ExecutableName = "opencode", InstallUrl = "https://opencode.ai", DocsUrl = "https://opencode.ai/docs" },
        new() { Id = "claude", DisplayName = "Claude Code", ExecutableName = "claude", InstallUrl = "https://docs.anthropic.com/en/docs/claude-code", DocsUrl = "https://docs.anthropic.com/en/docs/claude-code" },
        new() { Id = "codex", DisplayName = "OpenAI Codex CLI", ExecutableName = "codex", InstallUrl = "https://platform.openai.com/docs/codex", DocsUrl = "https://platform.openai.com/docs/codex" },
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
