using ChemResearchHub.Domain.Enums.Meetings;

namespace ChemResearchHub.Application.Meetings.DTOs;

public class MeetingSessionListItemDto
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime ScheduledStartAt { get; set; }

    public int PlannedDurationMinutes { get; set; }

    public MeetingSessionStatus Status { get; set; }

    public int AgendaItemCount { get; set; }

    public string? FacilitatorUserId { get; set; }
}