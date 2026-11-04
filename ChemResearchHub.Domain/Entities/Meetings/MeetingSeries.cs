using ChemResearchHub.Domain.Common;

namespace ChemResearchHub.Domain.Entities.Meetings;

public class MeetingSeries : BaseEntity
{
    private MeetingSeries()
    {
    }

    public MeetingSeries(
        string title,
        string? description = null,
        int defaultDurationMinutes = 120)
    {
        SetTitle(title);

        if (defaultDurationMinutes <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(defaultDurationMinutes));

        Description = description?.Trim();
        DefaultDurationMinutes = defaultDurationMinutes;
        IsActive = true;
    }

    public string Title { get; private set; } = null!;

    public string? Description { get; private set; }

    public int DefaultDurationMinutes { get; private set; }

    public bool IsActive { get; private set; }

    public ICollection<MeetingSession> Sessions { get; private set; }
        = new List<MeetingSession>();

    public void Update(
        string title,
        string? description,
        int defaultDurationMinutes)
    {
        SetTitle(title);

        if (defaultDurationMinutes <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(defaultDurationMinutes));

        Description = description?.Trim();
        DefaultDurationMinutes = defaultDurationMinutes;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        ModifiedAt = DateTime.UtcNow;
    }

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(
                "Meeting series title is required.",
                nameof(title));

        Title = title.Trim();
    }
}