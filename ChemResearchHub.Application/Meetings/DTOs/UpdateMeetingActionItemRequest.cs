using System.ComponentModel.DataAnnotations;

namespace ChemResearchHub.Application.Meetings.DTOs;

public class UpdateMeetingActionItemRequest
{
    [Required]
    [StringLength(250, MinimumLength = 2)]
    public string Title { get; set; } = null!;

    [StringLength(4000)]
    public string? Description { get; set; }

    public string? AssignedToUserId { get; set; }

    public DateTime? DueDate { get; set; }

    public int? ProjectId { get; set; }

    public int? DecisionId { get; set; }

    [StringLength(2000)]
    public string? AcceptanceCriteria { get; set; }

    // null یعنی Action Item بدون اتصال باشد.
    public int? WorkItemId { get; set; }
}