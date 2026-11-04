using ChemResearchHub.Domain.Enums.Meetings;
using System.ComponentModel.DataAnnotations;

namespace ChemResearchHub.Application.Meetings.DTOs;

public class CreateAgendaItemRequest
{
    [Required]
    [StringLength(250, MinimumLength = 2)]
    public string Title { get; set; } = null!;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Range(1, 720)]
    public int DurationMinutes { get; set; } = 10;

    public AgendaItemType Type { get; set; }
        = AgendaItemType.ResearchDiscussion;

    public AgendaItemPriority Priority { get; set; }
        = AgendaItemPriority.Planned;

    public string? PresenterUserId { get; set; }

    public int? ProjectId { get; set; }

    [StringLength(2000)]
    public string? ExpectedOutcome { get; set; }
}