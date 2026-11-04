using ChemResearchHub.Domain.Enums.Meetings;

namespace ChemResearchHub.Application.Meetings.DTOs;

public class MeetingActionItemDto
{
    public int Id { get; set; }

    public int MeetingSessionId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? AssignedToUserId { get; set; }

    public DateTime? DueDate { get; set; }

    public int? ProjectId { get; set; }

    public int? MeetingDecisionId { get; set; }

    public string? AcceptanceCriteria { get; set; }

    public MeetingActionItemStatus Status { get; set; }

    public MeetingActionItemType Type { get; set; }

    public int? WorkItemId { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}