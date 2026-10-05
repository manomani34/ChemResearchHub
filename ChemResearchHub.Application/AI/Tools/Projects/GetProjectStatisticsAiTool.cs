using System.Text.Json;

namespace ChemResearchHub.Application.AI.Tools.Projects;

public class GetProjectStatisticsAiTool : IAiTool
{
    private readonly ProjectAiContext _projectAiContext;

    public GetProjectStatisticsAiTool(
        ProjectAiContext projectAiContext)
    {
        _projectAiContext = projectAiContext;
    }

    public string Name => "get_project_statistics";

    public string Description =>
        "Gets statistical information about research projects in the ChemResearchHub database, including total, active, and inactive project counts.";

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
                    properties = new { },
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
            await _projectAiContext.GetProjectStatisticsAsync(
                cancellationToken);

        return JsonSerializer.Serialize(result);
    }
}