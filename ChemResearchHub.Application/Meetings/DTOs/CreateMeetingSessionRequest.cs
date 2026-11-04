using System.ComponentModel.DataAnnotations;

namespace ChemResearchHub.Application.Meetings.DTOs;

public class CreateMeetingSessionRequest
{
    [Required]
    [StringLength(250, MinimumLength = 3)]
    public string Title { get; set; } = null!;

    [StringLength(4000)]
    public string? Description { get; set; }

    public int? MeetingSeriesId { get; set; }

    [Required]
    public DateTime ScheduledStartAt { get; set; }

    [Range(1, 720)]
    public int PlannedDurationMinutes { get; set; } = 120;

    public string? FacilitatorUserId { get; set; }
}