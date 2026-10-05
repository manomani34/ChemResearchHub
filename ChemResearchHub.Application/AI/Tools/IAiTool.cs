namespace ChemResearchHub.Application.AI.Tools;

public interface IAiTool
{
    string Name { get; }

    string Description { get; }

    object GetDefinition();

    Task<string> ExecuteAsync(
        string arguments,
        CancellationToken cancellationToken = default);
}

