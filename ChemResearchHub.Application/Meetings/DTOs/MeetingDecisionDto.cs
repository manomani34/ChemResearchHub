using ChemResearchHub.Domain.Enums.Meetings;

namespace ChemResearchHub.Application.Meetings.DTOs;

public class MeetingDecisionDto
{
    public int Id { get; set; }

    public int MeetingSessionId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string DecidedByUserId { get; set; } = null!;

    public int? AgendaItemId { get; set; }

    public int? ProjectId { get; set; }

    public MeetingDecisionStatus Status { get; set; }

    public int? SupersededByDecisionId { get; set; }

    public DateTime CreatedAt { get; set; }
}