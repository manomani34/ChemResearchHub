using System.Text.Json;
using ChemResearchHub.Application.AI;
using ChemResearchHub.Application.AI.Tools;

namespace ChemResearchHub.Application.AI.Tools.Samples;

public class GetSampleAiTool : IAiTool
{
    private readonly ProjectAiContext _context;

    public GetSampleAiTool(ProjectAiContext context)
    {
        _context = context;
    }

    public string Name => "get_sample";

    public string Description =>
        "Gets detailed information about one specific sample. " +
        "Use this tool when the user asks for information or details " +
        "about a specific sample by its sample code.";

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
                        sampleCode = new
                        {
                            type = "string",
                            description = "The exact sample code."
                        }
                    },
                    required = new[]
                    {
                        "sampleCode"
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
        Console.WriteLine("===== GET SAMPLE TOOL =====");
        Console.WriteLine($"RAW ARGUMENTS: {arguments}");

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var input =
            JsonSerializer.Deserialize<Input>(
                arguments,
                options);

        if (input is null ||
            string.IsNullOrWhiteSpace(input.SampleCode))
        {
            return JsonSerializer.Serialize(new
            {
                found = false,
                message = "Sample code is required."
            });
        }

        var sampleCode =
            input.SampleCode.Trim();

        Console.WriteLine(
            $"LOOKING FOR SAMPLE: [{sampleCode}]");

        var result =
            await _context.GetSampleByCodeAsync(
                sampleCode,
                cancellationToken);

        if (result is null)
        {
            return JsonSerializer.Serialize(new
            {
                found = false,
                sampleCode,
                message = "No matching sample was found."
            });
        }

        return JsonSerializer.Serialize(
            result,
            options);
    }

    private class Input
    {
        public string SampleCode { get; set; } = string.Empty;
    }
}