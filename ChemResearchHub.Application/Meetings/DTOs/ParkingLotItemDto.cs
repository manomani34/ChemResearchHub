using ChemResearchHub.Domain.Enums.Meetings;

namespace ChemResearchHub.Application.Meetings.DTOs;

public class ParkingLotItemDto
{
    public int Id { get; set; }

    public int MeetingSessionId { get; set; }

    public string Title { get; set; } = null!;

    public string? Summary { get; set; }

    public string RaisedByUserId { get; set; } = null!;

    public int? AgendaItemId { get; set; }

    public int? ProjectId { get; set; }

    public ParkingLotStatus Status { get; set; }

    public int Priority { get; set; }

    public string? AssignedToUserId { get; set; }

    public int? TargetMeetingSessionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }
}