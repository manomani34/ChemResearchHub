namespace ChemResearchHub.Application.Meetings.DTOs;

public class LiveMeetingSnapshotDto
{
    public int MeetingSessionId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string MeetingStatus { get; set; } = string.Empty;

    public string TimerState { get; set; } = string.Empty;

    public DateTime ServerTimeUtc { get; set; }

    public DateTime? ActualStartedAt { get; set; }

    public DateTime? ActualEndedAt { get; set; }

    public int PlannedDurationMinutes { get; set; }

    public int ElapsedSeconds { get; set; }

    public int RemainingSeconds { get; set; }

    public int CurrentAgendaItemIndex { get; set; }

    public int? CurrentAgendaItemId { get; set; }

    public string? CurrentAgendaItemTitle { get; set; }

    public string? CurrentAgendaItemStatus { get; set; }

    public int? CurrentAgendaItemRemainingSeconds { get; set; }

    public int? CurrentAgendaItemDurationSeconds { get; set; }

    public List<LiveAgendaItemDto> AgendaItems { get; set; } = new();
}

public class LiveAgendaItemDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public int DurationMinutes { get; set; }

    public int ExtendedSeconds { get; set; }

    public int SortOrder { get; set; }

    public string? PresenterUserId { get; set; }

    public DateTime? ActualStartedAt { get; set; }

    public DateTime? ActualEndedAt { get; set; }

    public int? RemainingSeconds { get; set; }
}