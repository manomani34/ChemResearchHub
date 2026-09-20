using ChemResearchHub.Domain.Common;

namespace ChemResearchHub.Domain.Entities.Experiment;

public class Experiment : BaseEntity
{
    private Experiment()
    {
    }

    public Experiment(
        int workItemId,
        string title)
    {
        if (workItemId <= 0)
        {
            throw new ArgumentException(
                "WorkItemId must be greater than zero.",
                nameof(workItemId));
        }

        SetTitle(title);

        WorkItemId = workItemId;
    }

    public int WorkItemId { get; private set; }

    public string Title { get; private set; } = null!;

    public string? Description { get; private set; }

    public string? Protocol { get; private set; }

    public DateTime? StartedAt { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public string? Notes { get; private set; }

    public void Update(
        string title,
        string? description,
        string? protocol,
        DateTime? startedAt,
        DateTime? completedAt,
        string? notes)
    {
        SetTitle(title);

        Description = description;
        Protocol = protocol;
        StartedAt = startedAt;
        CompletedAt = completedAt;
        Notes = notes;

        ModifiedAt = DateTime.UtcNow;
    }

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "Experiment title is required.",
                nameof(title));
        }

        Title = title.Trim();
    }
}