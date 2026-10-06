using ChemResearchHub.Application.AI.Ollama;
using System.Text.Json;

namespace ChemResearchHub.Application.AI.Tools;

public class AiToolExecutor
{
    private readonly AiToolRegistry _toolRegistry;

    public AiToolExecutor(
        AiToolRegistry toolRegistry)
    {
        _toolRegistry = toolRegistry;
    }

    public async Task<string?> ExecuteAsync(
        OllamaToolCall toolCall,
        CancellationToken cancellationToken = default)
    {
        var toolName =
            toolCall.Function?.Name;

        if (string.IsNullOrWhiteSpace(toolName))
            return null;

        var tool =
            _toolRegistry.GetTool(toolName);

        if (tool is null)
            return null;

        var arguments =
            toolCall.Function.Arguments.GetRawText();

        return await tool.ExecuteAsync(
            arguments,
            cancellationToken);
    }
}