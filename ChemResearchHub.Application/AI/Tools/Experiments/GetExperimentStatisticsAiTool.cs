using System.Text.Json;

namespace ChemResearchHub.Application.AI.Tools.Experiments;

public class GetExperimentStatisticsAiTool : IAiTool
{
    private readonly ProjectAiContext _projectAiContext;

    public GetExperimentStatisticsAiTool(
        ProjectAiContext projectAiContext)
    {
        _projectAiContext = projectAiContext;
    }

    public string Name => "get_experiment_statistics";

    public string Description =>
        "Gets statistics about experiments in the ChemResearchHub database, including total, completed, in-progress, and not-started experiments.";

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
            await _projectAiContext.GetExperimentStatisticsAsync(
                cancellationToken);

        return JsonSerializer.Serialize(new
        {
            success = true,
            result
        });
    }
}