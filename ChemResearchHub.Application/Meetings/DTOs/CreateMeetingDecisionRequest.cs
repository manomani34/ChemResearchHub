using System.ComponentModel.DataAnnotations;

namespace ChemResearchHub.Application.Meetings.DTOs;

public class CreateMeetingDecisionRequest
{
    [Required]
    [StringLength(250, MinimumLength = 2)]
    public string Title { get; set; } = null!;

    [StringLength(4000)]
    public string? Description { get; set; }

    public int? AgendaItemId { get; set; }

    public int? ProjectId { get; set; }
}