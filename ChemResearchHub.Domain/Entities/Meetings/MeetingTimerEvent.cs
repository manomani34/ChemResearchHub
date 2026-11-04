using ChemResearchHub.Domain.Common;
using ChemResearchHub.Domain.Enums.Meetings;

namespace ChemResearchHub.Domain.Entities.Meetings;

public class MeetingTimerEvent : BaseEntity
{
    private MeetingTimerEvent()
    {
    }

    public MeetingTimerEvent(
        int meetingSessionId,
        MeetingTimerEventType eventType,
        string performedByUserId,
        int? agendaItemId = null,
        int? durationSeconds = null,
        string? description = null,
        string? correlationId = null)
    {
        if (meetingSessionId <= 0)
            throw new ArgumentOutOfRangeException(nameof(meetingSessionId));

        if (string.IsNullOrWhiteSpace(performedByUserId))
            throw new ArgumentException(
                "Performed by user id is required.",
                nameof(performedByUserId));

        if (durationSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(durationSeconds));

        MeetingSessionId = meetingSessionId;
        EventType = eventType;
        PerformedByUserId = performedByUserId.Trim();

        AgendaItemId = agendaItemId;
        DurationSeconds = durationSeconds;
        Description = description?.Trim();
        CorrelationId = Normalize(correlationId);

        OccurredAt = DateTime.UtcNow;
    }

    public int MeetingSessionId { get; private set; }

    public MeetingSession MeetingSession { get; private set; } = null!;

    public int? AgendaItemId { get; private set; }

    public MeetingTimerEventType EventType { get; private set; }

    public string PerformedByUserId { get; private set; } = null!;

    public DateTime OccurredAt { get; private set; }

    public int? DurationSeconds { get; private set; }

    public string? Description { get; private set; }

    public string? CorrelationId { get; private set; }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}