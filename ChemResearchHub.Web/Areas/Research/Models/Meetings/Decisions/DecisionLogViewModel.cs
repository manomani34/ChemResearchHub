using ChemResearchHub.Application.Meetings.DTOs;
using ChemResearchHub.Domain.Enums.Meetings;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ChemResearchHub.Web.Areas.Research.Models.Meetings.Decisions;

public class DecisionLogViewModel
{
    public int MeetingSessionId { get; set; }

    public List<MeetingDecisionDto> Decisions { get; set; } = [];

    public CreateMeetingDecisionRequest CreateRequest { get; set; } = new();

    public List<SelectListItem> Projects { get; set; } = [];

    public List<SelectListItem> AgendaItems { get; set; } = [];

    public int TotalCount => Decisions.Count;

    public int ActiveCount =>
        Decisions.Count(x => x.Status == MeetingDecisionStatus.Active);

    public int SupersededCount =>
        Decisions.Count(x => x.Status == MeetingDecisionStatus.Superseded);

    public int ArchivedCount =>
        Decisions.Count(x => x.Status == MeetingDecisionStatus.Archived);
}