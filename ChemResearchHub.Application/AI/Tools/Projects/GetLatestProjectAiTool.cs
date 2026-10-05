using System.Text.Json;

namespace ChemResearchHub.Application.AI.Tools.Projects;

public class GetLatestProjectAiTool : IAiTool
{
    private readonly ProjectAiContext _projectAiContext;

    public GetLatestProjectAiTool(
        ProjectAiContext projectAiContext)
    {
        _projectAiContext = projectAiContext;
    }

    public string Name => "get_latest_project";

    public string Description =>
        "Gets the most recently created research project from the ChemResearchHub database. The latest project is determined by CreatedAt.";

    public object GetDefinition()
    {
        return new
        {
            type = "function",

            function = new
            {
                name = Name,
                description = Description,

                parameters = new
                {
                    type = "object",

                    properties = new
                    {
                    },

                    required = Array.Empty<string>()
                }
            }
        };
    }

    public async Task<string> ExecuteAsync(
        string arguments,
        CancellationToken cancellationToken = default)
    {
        var result =
            await _projectAiContext.GetLatestProjectAsync(
                cancellationToken);

        return JsonSerializer.Serialize(result);
    }
}