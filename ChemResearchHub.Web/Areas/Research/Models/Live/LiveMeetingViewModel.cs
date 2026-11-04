using ChemResearchHub.Application.Meetings.DTOs;

namespace ChemResearchHub.Web.Areas.Research.Models.Meetings.Live;

public class LiveMeetingViewModel
{
    public MeetingSessionDto Meeting { get; set; } = null!;

    public bool CanControl { get; set; }

    public bool IsPresentation { get; set; }

    public string CurrentUserId { get; set; } = string.Empty;

    public string HubUrl { get; set; } = string.Empty;
}