using System.Text.Json;

namespace ChemResearchHub.Application.AI.Tools.Experiments;

public class GetLatestExperimentAiTool : IAiTool
{
    private readonly ProjectAiContext _projectAiContext;

    public GetLatestExperimentAiTool(
        ProjectAiContext projectAiContext)
    {
        _projectAiContext = projectAiContext;
    }

    public string Name => "get_latest_experiment";

    public string Description =>
        "Gets the most recently started experiment from the ChemResearchHub database.";

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
            await _projectAiContext.GetLatestExperimentAsync(
                cancellationToken);

        if (result is null)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                message = "No experiments were found."
            });
        }

        return JsonSerializer.Serialize(new
        {
            success = true,
            experiment = result
        });
    }
}