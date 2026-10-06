using ChemResearchHub.Application.AI;

namespace ChemResearchHub.Application.AI.Routing;

public class AiRequestRouter : IAiRequestRouter
{
    public AiIntent Detect(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return AiIntent.General;

        var text =
            message.Trim().ToLowerInvariant();

        if (
            text.Contains("نمونه") ||
            text.Contains("sample") ||
            text.Contains("samples")
        )
        {
            return AiIntent.Sample;
        }

        if (
            text.Contains("آزمایش") ||
            text.Contains("experiment") ||
            text.Contains("experiments")
        )
        {
            return AiIntent.Experiment;
        }

        if (
            text.Contains("پروژه") ||
            text.Contains("project") ||
            text.Contains("projects")
        )
        {
            return AiIntent.Project;
        }

        return AiIntent.General;
    }
}