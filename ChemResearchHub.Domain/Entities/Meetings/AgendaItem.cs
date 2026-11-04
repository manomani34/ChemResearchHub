using ChemResearchHub.Domain.Common;
using ChemResearchHub.Domain.Enums.Meetings;

namespace ChemResearchHub.Domain.Entities.Meetings;

public class AgendaItem : BaseEntity
{
    private AgendaItem()
    {
    }

    public AgendaItem(
        int meetingSessionId,
        string title,
        int durationMinutes,
        int sortOrder,
        AgendaItemType type = AgendaItemType.ResearchDiscussion,
        AgendaItemPriority priority = AgendaItemPriority.Planned,
        string? description = null,
        string? presenterUserId = null,
        int? projectId = null,
        string? expectedOutcome = null)
    {
        if (meetingSessionId <= 0)
            throw new ArgumentOutOfRangeException(nameof(meetingSessionId));

        if (durationMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationMinutes));

        if (sortOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(sortOrder));

        MeetingSessionId = meetingSessionId;
        SetTitle(title);

        DurationMinutes = durationMinutes;
        SortOrder = sortOrder;
        Type = type;
        Priority = priority;

        Description = description?.Trim();
        PresenterUserId = Normalize(presenterUserId);
        ProjectId = projectId;
        ExpectedOutcome = expectedOutcome?.Trim();

        Status = AgendaItemStatus.Pending;
    }

    public int MeetingSessionId { get; private set; }

    public MeetingSession MeetingSession { get; private set; } = null!;

    public string Title { get; private set; } = null!;

    public string? Description { get; private set; }

    public AgendaItemType Type { get; private set; }

    public AgendaItemPriority Priority { get; private set; }

    public AgendaItemStatus Status { get; private set; }

    public int DurationMinutes { get; private set; }

    public int SortOrder { get; private set; }

    public string? PresenterUserId { get; private set; }

    public int? ProjectId { get; private set; }

    public string? ExpectedOutcome { get; private set; }

    public DateTime? ActualStartedAt { get; private set; }

    public DateTime? ActualEndedAt { get; private set; }

    public int ExtendedSeconds { get; private set; }

    public void Update(
        string title,
        string? description,
        int durationMinutes,
        AgendaItemType type,
        AgendaItemPriority priority,
        string? presenterUserId,
        int? projectId,
        string? expectedOutcome,
        int sortOrder)
    {
        if (Status != AgendaItemStatus.Pending)
            throw new InvalidOperationException(
                "Only pending agenda items can be edited.");

        SetTitle(title);

        if (durationMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationMinutes));

        if (sortOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(sortOrder));

        Description = description?.Trim();
        DurationMinutes = durationMinutes;
        Type = type;
        Priority = priority;
        PresenterUserId = Normalize(presenterUserId);
        ProjectId = projectId;
        ExpectedOutcome = expectedOutcome?.Trim();
        SortOrder = sortOrder;

        ModifiedAt = DateTime.UtcNow;
    }

    public void Start(DateTime? now = null)
    {
        if (Status != AgendaItemStatus.Pending)
            throw new InvalidOperationException(
                "Agenda item cannot be started.");

        ActualStartedAt = now ?? DateTime.UtcNow;
        Status = AgendaItemStatus.InProgress;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Extend(int additionalSeconds)
    {
        if (Status != AgendaItemStatus.InProgress &&
            Status != AgendaItemStatus.Overtime)
        {
            throw new InvalidOperationException(
                "Only active agenda items can be extended.");
        }

        if (additionalSeconds <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(additionalSeconds));

        ExtendedSeconds += additionalSeconds;
        ModifiedAt = DateTime.UtcNow;
    }

    public void MarkOvertime()
    {
        if (Status != AgendaItemStatus.InProgress)
            return;

        Status = AgendaItemStatus.Overtime;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Complete(DateTime? now = null)
    {
        if (Status != AgendaItemStatus.InProgress &&
            Status != AgendaItemStatus.Overtime)
        {
            throw new InvalidOperationException(
                "Agenda item is not active.");
        }

        ActualEndedAt = now ?? DateTime.UtcNow;
        Status = AgendaItemStatus.Completed;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Skip()
    {
        if (Status == AgendaItemStatus.Completed)
            throw new InvalidOperationException(
                "Completed agenda item cannot be skipped.");

        Status = AgendaItemStatus.Skipped;
        ActualEndedAt = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;
    }

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(
                "Agenda item title is required.",
                nameof(title));

        Title = title.Trim();
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}