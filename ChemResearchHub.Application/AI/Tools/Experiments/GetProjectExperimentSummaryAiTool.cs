using System.Text.Json;

namespace ChemResearchHub.Application.AI.Tools.Experiments;

public class GetProjectExperimentSummaryAiTool : IAiTool
{
    private readonly ProjectAiContext _projectAiContext;

    public GetProjectExperimentSummaryAiTool(
        ProjectAiContext projectAiContext)
    {
        _projectAiContext = projectAiContext;
    }

    public string Name => "get_project_experiment_summary";

    public string Description =>
        "Gets a summary of experiment statistics for a specific research project, including total, completed, in-progress, and not-started experiments.";

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
                        projectName = new
                        {
                            type = "string",
                            description =
                                "The name of the research project."
                        }
                    },
                    required = new[] { "projectName" }
                }
            }
        };
    }

    public async Task<string> ExecuteAsync(
        string arguments,
        CancellationToken cancellationToken = default)
    {
        var parameters =
            JsonSerializer.Deserialize<
                GetProjectExperimentSummaryArguments>(
                    arguments,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

        if (parameters is null ||
            string.IsNullOrWhiteSpace(parameters.ProjectName))
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                message = "Project name is required."
            });
        }

        var result =
            await _projectAiContext
                .GetProjectExperimentSummaryAsync(
                    parameters.ProjectName,
                    cancellationToken);

        if (result is null)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                message =
                    $"No project was found with the name '{parameters.ProjectName}'."
            });
        }

        return JsonSerializer.Serialize(new
        {
            success = true,
            result
        });
    }

    private class GetProjectExperimentSummaryArguments
    {
        public string ProjectName { get; set; } = string.Empty;
    }
}