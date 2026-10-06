using ChemResearchHub.Application.AI;

namespace ChemResearchHub.Application.AI.Tools;

public class AiToolSelector
{
    private readonly AiToolRegistry _toolRegistry;

    public AiToolSelector(
        AiToolRegistry toolRegistry)
    {
        _toolRegistry = toolRegistry;
    }

    public object[] GetToolsForIntent(
        AiIntent intent)
    {
        return intent switch
        {
            AiIntent.Experiment =>
                GetToolsContaining("experiment"),

            AiIntent.Project =>
                GetToolsContaining("project"),

            _ =>
                _toolRegistry.GetDefinitions()
        };
    }

    private object[] GetToolsContaining(
        string keyword)
    {
        return _toolRegistry.Tools
            .Where(x =>
                x.Name.Contains(
                    keyword,
                    StringComparison.OrdinalIgnoreCase))
            .Select(x => x.GetDefinition())
            .ToArray();
    }
}