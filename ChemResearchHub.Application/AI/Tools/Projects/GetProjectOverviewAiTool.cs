using System.Text.Json;

namespace ChemResearchHub.Application.AI.Tools.Projects;

public class GetProjectOverviewAiTool : IAiTool
{
    private readonly ProjectAiContext _projectAiContext;

    public GetProjectOverviewAiTool(
        ProjectAiContext projectAiContext)
    {
        _projectAiContext = projectAiContext;
    }

    public string Name => "get_project_overview";

    public string Description =>
        "Gets a research project's information and calculates its current time-based status, including elapsed days, remaining days, and time progress percentage.";

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
                        name = new
                        {
                            type = "string",
                            description =
                                "The name of the research project."
                        }
                    },
                    required = new[] { "name" }
                }
            }
        };
    }

    public async Task<string> ExecuteAsync(
        string arguments,
        CancellationToken cancellationToken = default)
    {
        var parameters =
            JsonSerializer.Deserialize<GetProjectOverviewArguments>(
                arguments,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (parameters is null ||
            string.IsNullOrWhiteSpace(parameters.Name))
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                message = "Project name is required."
            });
        }

        var result =
            await _projectAiContext.GetProjectOverviewAsync(
                parameters.Name,
                cancellationToken);

        if (result is null)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                message =
                    $"No project was found with the name '{parameters.Name}'."
            });
        }

        return JsonSerializer.Serialize(new
        {
            success = true,
            project = result
        });
    }

    private class GetProjectOverviewArguments
    {
        public string Name { get; set; } = string.Empty;
    }
}