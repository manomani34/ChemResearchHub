using ChemResearchHub.Application.Meetings.DTOs;
using ChemResearchHub.Domain.Enums.Meetings;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ChemResearchHub.Web.Areas.Research.Models.Meetings.ActionItems;

public class ActionItemsViewModel
{
    public int MeetingSessionId { get; set; }

    public List<MeetingActionItemDto> Items { get; set; } = [];

    public CreateMeetingActionItemRequest CreateRequest { get; set; } = new();

    public List<SelectListItem> Users { get; set; } = [];

    public List<SelectListItem> Projects { get; set; } = [];

    public List<SelectListItem> Decisions { get; set; } = [];

    public List<SelectListItem> WorkItems { get; set; } = [];

    public int TotalCount => Items.Count;

    public int OpenCount =>
        Items.Count(x => x.Status == MeetingActionItemStatus.Open);

    public int InProgressCount =>
        Items.Count(x => x.Status == MeetingActionItemStatus.InProgress);

    public int BlockedCount =>
        Items.Count(x => x.Status == MeetingActionItemStatus.Blocked);

    public int CompletedCount =>
        Items.Count(x => x.Status == MeetingActionItemStatus.Completed);
}