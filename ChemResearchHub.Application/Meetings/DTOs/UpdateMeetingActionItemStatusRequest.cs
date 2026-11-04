using ChemResearchHub.Domain.Enums.Meetings;

namespace ChemResearchHub.Application.Meetings.DTOs;

public class UpdateMeetingActionItemStatusRequest
{
    public MeetingActionItemStatus Status { get; set; }
}