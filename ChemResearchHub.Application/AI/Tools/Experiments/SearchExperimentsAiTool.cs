using System.Text.Json;

namespace ChemResearchHub.Application.AI.Tools.Experiments;

public class SearchExperimentsAiTool : IAiTool
{
    private readonly ProjectAiContext _projectAiContext;

    public SearchExperimentsAiTool(
        ProjectAiContext projectAiContext)
    {
        _projectAiContext = projectAiContext;
    }

    public string Name => "search_experiments";

    public string Description =>
        "Searches experiments by matching a search term against experiment titles, descriptions, protocols, and notes.";

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
                                "A keyword or phrase to search for in experiment titles, descriptions, protocols, or notes."
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
            JsonSerializer.Deserialize<SearchExperimentsArguments>(
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
            await _projectAiContext.SearchExperimentsAsync(
                parameters.SearchTerm,
                cancellationToken);

        return JsonSerializer.Serialize(new
        {
            success = true,
            result
        });
    }

    private class SearchExperimentsArguments
    {
        public string SearchTerm { get; set; } = string.Empty;
    }
}