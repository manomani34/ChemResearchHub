using ChemResearchHub.Domain.Enums.Meetings;

namespace ChemResearchHub.Application.Meetings.DTOs;

public class AgendaItemDto
{
    public int Id { get; set; }

    public int MeetingSessionId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public AgendaItemType Type { get; set; }

    public AgendaItemPriority Priority { get; set; }

    public AgendaItemStatus Status { get; set; }

    public int DurationMinutes { get; set; }

    public int SortOrder { get; set; }

    public string? PresenterUserId { get; set; }

    public int? ProjectId { get; set; }

    public string? ExpectedOutcome { get; set; }

    public DateTime? ActualStartedAt { get; set; }

    public DateTime? ActualEndedAt { get; set; }

    public int ExtendedSeconds { get; set; }
}