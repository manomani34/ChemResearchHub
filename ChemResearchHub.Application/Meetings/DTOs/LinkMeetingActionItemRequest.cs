using System.ComponentModel.DataAnnotations;

namespace ChemResearchHub.Application.Meetings.DTOs;

public class LinkMeetingActionItemRequest
{
    [Range(1, int.MaxValue)]
    public int WorkItemId { get; set; }
}