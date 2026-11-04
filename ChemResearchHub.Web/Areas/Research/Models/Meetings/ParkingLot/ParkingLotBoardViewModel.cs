using ChemResearchHub.Application.Meetings.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ChemResearchHub.Web.Areas.Research.Models.Meetings.ParkingLot;

public class ParkingLotBoardViewModel
{
    public int MeetingSessionId { get; set; }

    public List<ParkingLotItemDto> Items { get; set; } = [];

    public CreateParkingLotRequest CreateRequest { get; set; } = new();

    public List<SelectListItem> Projects { get; set; } = [];

    public List<SelectListItem> AgendaItems { get; set; } = [];

    public int TotalCount => Items.Count;

    public int SubmittedCount =>
        Items.Count(x => x.Status == Domain.Enums.Meetings.ParkingLotStatus.Submitted);

    public int UnderReviewCount =>
        Items.Count(x => x.Status == Domain.Enums.Meetings.ParkingLotStatus.UnderReview);

    public int ResolvedCount =>
        Items.Count(x => x.Status == Domain.Enums.Meetings.ParkingLotStatus.Resolved);
}