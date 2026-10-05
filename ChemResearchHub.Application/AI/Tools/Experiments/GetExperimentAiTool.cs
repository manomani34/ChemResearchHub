using System.Text.Json;

namespace ChemResearchHub.Application.AI.Tools.Experiments;

public class GetExperimentAiTool : IAiTool
{
    private readonly ProjectAiContext _projectAiContext;

    public GetExperimentAiTool(
        ProjectAiContext projectAiContext)
    {
        _projectAiContext = projectAiContext;
    }

    public string Name => "get_experiment";

    public string Description =>
        "Gets a specific experiment from the ChemResearchHub database by its title.";

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
                        title = new
                        {
                            type = "string",
                            description =
                                "The exact title of the experiment."
                        }
                    },
                    required = new[] { "title" }
                }
            }
        };
    }

    public async Task<string> ExecuteAsync(
        string arguments,
        CancellationToken cancellationToken = default)
    {
        var parameters =
            JsonSerializer.Deserialize<GetExperimentArguments>(
                arguments,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (parameters is null ||
            string.IsNullOrWhiteSpace(parameters.Title))
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                message = "Experiment title is required."
            });
        }

        var experiment =
            await _projectAiContext.GetExperimentByTitleAsync(
                parameters.Title,
                cancellationToken);

        if (experiment is null)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                message =
                    $"No experiment was found with the title '{parameters.Title}'."
            });
        }

        return JsonSerializer.Serialize(new
        {
            success = true,
            experiment
        });
    }

    private class GetExperimentArguments
    {
        public string Title { get; set; } = string.Empty;
    }
}