using System.Text.Json;
using ChemResearchHub.Application.AI.Tools;
using ChemResearchHub.Application.AI.Tools.Projects;

namespace ChemResearchHub.Application.AI.Tools.Samples;

public class GetSamplesByExperimentAiTool : IAiTool
{
    private readonly ProjectAiContext _context;

    public GetSamplesByExperimentAiTool(
        ProjectAiContext context)
    {
        _context = context;
    }

    public string Name => "get_samples_by_experiment";

    public string Description =>
        "Gets the samples INSIDE a specific experiment. " +
        "Use this tool when the user asks for samples, sample codes, " +
        "or sample information belonging to an experiment. " +
        "IMPORTANT: Do NOT use search_experiments for sample requests. " +
        "search_experiments finds experiment records; this tool finds " +
        "the samples belonging to an experiment.";

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
                        experimentTitle = new
                        {
                            type = "string",
                            description =
                                "The exact title of the experiment."
                        }
                    },
                    required = new[]
                    {
                        "experimentTitle"
                    }
                }
            }
        };
    }

    public async Task<string> ExecuteAsync(
        string arguments,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine();
        Console.WriteLine("===== SAMPLE TOOL CALLED =====");
        Console.WriteLine($"RAW ARGUMENTS: {arguments}");

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var input =
            JsonSerializer.Deserialize<Input>(
                arguments,
                options);

        Console.WriteLine(
            $"EXPERIMENT TITLE: [{input?.ExperimentTitle}]");

        if (input is null ||
            string.IsNullOrWhiteSpace(input.ExperimentTitle))
        {
            Console.WriteLine(
                "ERROR: Experiment title is empty.");

            Console.WriteLine(
                "==============================");

            return JsonSerializer.Serialize(new
            {
                found = false,
                message = "Experiment title is required."
            });
        }

        var experimentTitle =
            input.ExperimentTitle.Trim();

        Console.WriteLine(
            $"LOOKING FOR EXPERIMENT: [{experimentTitle}]");

        var result =
            await _context.GetSamplesByExperimentTitleAsync(
                experimentTitle,
                cancellationToken);

        var serializedResult =
            JsonSerializer.Serialize(
                result,
                options);

        Console.WriteLine(
            $"TOOL RESULT: {serializedResult}");

        Console.WriteLine(
            "==============================");

        if (result is null)
        {
            return JsonSerializer.Serialize(new
            {
                found = false,
                experimentTitle,
                message =
                    "No matching experiment was found."
            });
        }

        return serializedResult;
    }

    private class Input
    {
        public string ExperimentTitle { get; set; } = string.Empty;
    }
}