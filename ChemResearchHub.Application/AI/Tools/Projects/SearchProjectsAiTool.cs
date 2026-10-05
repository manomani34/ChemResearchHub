using System.Text.Json;

namespace ChemResearchHub.Application.AI.Tools.Projects;

public class SearchProjectsAiTool : IAiTool
{
    private readonly ProjectAiContext _projectAiContext;

    public SearchProjectsAiTool(
        ProjectAiContext projectAiContext)
    {
        _projectAiContext = projectAiContext;
    }

    public string Name => "search_projects";

    public string Description =>
        "Searches research projects by matching the search term against project names and descriptions.";

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
                        searchTerm = new
                        {
                            type = "string",
                            description =
                                "A keyword or phrase to search for in project names and descriptions."
                        }
                    },
                    required = new[] { "searchTerm" }
                }
            }
        };
    }

    public async Task<string> ExecuteAsync(
        string arguments,
        CancellationToken cancellationToken = default)
    {
        var parameters =
            JsonSerializer.Deserialize<SearchProjectsArguments>(
                arguments,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (parameters is null ||
            string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                message = "Search term is required."
            });
        }

        var result =
            await _projectAiContext.SearchProjectsAsync(
                parameters.SearchTerm,
                cancellationToken);

        return JsonSerializer.Serialize(result);
    }

    private class SearchProjectsArguments
    {
        public string SearchTerm { get; set; } = string.Empty;
    }
}