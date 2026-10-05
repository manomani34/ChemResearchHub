using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChemResearchHub.Application.AI.Tools.Projects;

public class GetProjectAiTool : IAiTool
{
    private readonly ProjectAiContext _projectAiContext;

    public GetProjectAiTool(
        ProjectAiContext projectAiContext)
    {
        _projectAiContext = projectAiContext;
    }

    public string Name => "get_project";

    public string Description =>
        "Gets a specific research project from the ChemResearchHub database by its name.";

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

                    required = new[]
                    {
                        "name"
                    }
                }
            }
        };
    }

    public async Task<string> ExecuteAsync(
        string arguments,
        CancellationToken cancellationToken = default)
    {
        var parameters =
            JsonSerializer.Deserialize<GetProjectArguments>(
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

        var project =
            await _projectAiContext.GetProjectByNameAsync(
                parameters.Name,
                cancellationToken);

        if (project is null)
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
            project
        });
    }

    private class GetProjectArguments
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}