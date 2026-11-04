using ChemResearchHub.Domain.Enums.Meetings;
using System.ComponentModel.DataAnnotations;

namespace ChemResearchHub.Application.Meetings.DTOs;

public class UpdateAgendaItemRequest
{
    [Required]
    [StringLength(250, MinimumLength = 2)]
    public string Title { get; set; } = null!;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Range(1, 720)]
    public int DurationMinutes { get; set; }

    public AgendaItemType Type { get; set; }

    public AgendaItemPriority Priority { get; set; }

    public string? PresenterUserId { get; set; }

    public int? ProjectId { get; set; }

    [StringLength(2000)]
    public string? ExpectedOutcome { get; set; }

    [Range(0, 1000)]
    public int SortOrder { get; set; }
}