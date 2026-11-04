using System.Security.Claims;
using ChemResearchHub.Application.Meetings.Interfaces;
using ChemResearchHub.Application.Meetings.Repositories;
using ChemResearchHub.Web.Areas.Research.Hubs;
using ChemResearchHub.Web.Areas.Research.Models.Meetings.Live;
using ChemResearchHub.Web.Areas.Research.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChemResearchHub.Web.Areas.Research.Controllers;

[Area("Research")]
[Route("Meetings/{meetingSessionId:int}/Live")]
[Authorize]
public class MeetingLiveController : Controller
{
    private readonly IMeetingService _meetingService;
    private readonly IMeetingRepository _meetingRepository;

    public MeetingLiveController(
        IMeetingService meetingService,
        IMeetingRepository meetingRepository)
    {
        _meetingService = meetingService;
        _meetingRepository = meetingRepository;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        int meetingSessionId,
        [FromQuery] bool presentation = false,
        CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var session = await _meetingRepository.GetSessionByIdAsync(
            meetingSessionId,
            true,
            cancellationToken);

        if (session is null)
            return NotFound();

        if (!MeetingRoomAccess.CanJoin(session, userId))
            return Forbid();

        var meeting = await _meetingService.GetByIdAsync(
            meetingSessionId,
            cancellationToken);

        if (meeting is null)
            return NotFound();

        var model = new LiveMeetingViewModel
        {
            Meeting = meeting,
            CanControl = MeetingRoomAccess.CanControl(
                session,
                userId),
            IsPresentation = presentation,
            CurrentUserId = userId,
            HubUrl = LiveMeetingHub.HubUrl
        };

        return View(model);
    }
}