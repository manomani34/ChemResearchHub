namespace ChemResearchHub.Application.AI;

public enum AiIntent
{
    General,
    Project,
    Experiment,
    Sample
}

public class AiIntentRouter
{
    public AiIntent Detect(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return AiIntent.General;

        var text = message.Trim().ToLowerInvariant();

        // Sample
        if (
            text.Contains("نمونه") ||
            text.Contains("sample") ||
            text.Contains("samples")
        )
        {
            return AiIntent.Sample;
        }

        // Experiment
        if (
            text.Contains("آزمایش") ||
            text.Contains("experiment") ||
            text.Contains("experiments")
        )
        {
            return AiIntent.Experiment;
        }

        // Project
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