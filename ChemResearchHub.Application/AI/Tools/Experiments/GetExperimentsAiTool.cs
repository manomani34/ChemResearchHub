using System.Text.Json;

namespace ChemResearchHub.Application.AI.Tools.Experiments;

public class GetExperimentsAiTool : IAiTool
{
    private readonly ProjectAiContext _projectAiContext;

    public GetExperimentsAiTool(
        ProjectAiContext projectAiContext)
    {
        _projectAiContext = projectAiContext;
    }

    public string Name => "get_experiments";

    public string Description =>
        "Gets all experiments belonging to a specific research project. Use this tool when the user asks about experiments of a project.";

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
            JsonSerializer.Deserialize<GetExperimentsArguments>(
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
            await _projectAiContext.GetExperimentsByProjectNameAsync(
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

    private class GetExperimentsArguments
    {
        public string ProjectName { get; set; } = string.Empty;
    }
}