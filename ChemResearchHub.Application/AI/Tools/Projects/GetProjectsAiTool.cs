using System.Text.Json;

namespace ChemResearchHub.Application.AI.Tools.Projects;

public class GetProjectsAiTool : IAiTool
{
    private readonly ProjectAiContext _projectAiContext;

    public GetProjectsAiTool(
        ProjectAiContext projectAiContext)
    {
        _projectAiContext = projectAiContext;
    }

    public string Name => "get_projects";

    public string Description =>
        "Gets all research projects from the ChemResearchHub database.";

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
            await _projectAiContext.GetProjectsAsync(
                cancellationToken);

        return JsonSerializer.Serialize(result);
    }
}