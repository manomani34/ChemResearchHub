using ChemResearchHub.Domain.Enums.Meetings;

namespace ChemResearchHub.Application.Meetings.DTOs;

public class MeetingSessionDto
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int? MeetingSeriesId { get; set; }

    public DateTime ScheduledStartAt { get; set; }

    public int PlannedDurationMinutes { get; set; }

    public DateTime? ActualStartedAt { get; set; }

    public DateTime? ActualEndedAt { get; set; }

    public MeetingSessionStatus Status { get; set; }

    public MeetingTimerState TimerState { get; set; }

    public string? FacilitatorUserId { get; set; }

    public string? CreatedByUserId { get; set; }

    public int CurrentAgendaItemIndex { get; set; }

    public int ElapsedSeconds { get; set; }

    public int RemainingSeconds { get; set; }

    public List<AgendaItemDto> AgendaItems { get; set; } = new();
}