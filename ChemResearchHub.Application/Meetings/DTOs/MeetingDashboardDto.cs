namespace ChemResearchHub.Application.Meetings.DTOs;

public class MeetingDashboardDto
{
    public int TotalMeetings { get; set; }

    public int UpcomingMeetings { get; set; }

    public int CompletedMeetings { get; set; }

    public int InProgressMeetings { get; set; }

    public List<MeetingSessionListItemDto> Upcoming { get; set; } = new();

    public List<MeetingSessionListItemDto> Recent { get; set; } = new();
}