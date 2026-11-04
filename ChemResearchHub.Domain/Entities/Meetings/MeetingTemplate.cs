using ChemResearchHub.Domain.Common;
using ChemResearchHub.Domain.Enums.Meetings;

namespace ChemResearchHub.Domain.Entities.Meetings;

public class MeetingTemplate : BaseEntity
{
    private MeetingTemplate()
    {
    }

    public MeetingTemplate(
        string title,
        MeetingTemplateType type,
        string? description = null,
        int defaultDurationMinutes = 120)
    {
        SetTitle(title);

        if (defaultDurationMinutes <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(defaultDurationMinutes));

        Type = type;
        Description = description?.Trim();
        DefaultDurationMinutes = defaultDurationMinutes;
        IsActive = true;
    }

    public string Title { get; private set; } = null!;

    public string? Description { get; private set; }

    public MeetingTemplateType Type { get; private set; }

    public int DefaultDurationMinutes { get; private set; }

    public bool IsActive { get; private set; }

    public void Update(
        string title,
        string? description,
        MeetingTemplateType type,
        int defaultDurationMinutes)
    {
        SetTitle(title);

        if (defaultDurationMinutes <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(defaultDurationMinutes));

        Description = description?.Trim();
        Type = type;
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
                "Template title is required.",
                nameof(title));

        Title = title.Trim();
    }
}