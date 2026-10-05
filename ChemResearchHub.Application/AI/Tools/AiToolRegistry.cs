namespace ChemResearchHub.Application.AI.Tools;

public class AiToolRegistry
{
    private readonly Dictionary<string, IAiTool> _tools;

    public AiToolRegistry(IEnumerable<IAiTool> tools)
    {
        _tools = tools.ToDictionary(
            x => x.Name,
            StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<IAiTool> Tools =>
        _tools.Values;

    public IAiTool? GetTool(string name)
    {
        return _tools.TryGetValue(name, out var tool)
            ? tool
            : null;
    }

    public object[] GetDefinitions()
    {
        return _tools.Values
            .Select(x => x.GetDefinition())
            .ToArray();
    }
}
