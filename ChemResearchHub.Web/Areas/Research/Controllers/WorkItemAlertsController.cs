using ChemResearchHub.Application.WorkItemUserAlerts.Interfaces;
using ChemResearchHub.Web.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ChemResearchHub.Web.Areas.Research.Controllers;

[Area("Research")]
[Authorize]
[Route("Research/WorkItemAlerts")]
public class WorkItemAlertsController : Controller
{
    private readonly IWorkItemUserAlertService _alertService;
    private readonly IHubContext<WorkItemAlertHub> _hubContext;

    public WorkItemAlertsController(
        IWorkItemUserAlertService alertService,
        IHubContext<WorkItemAlertHub> hubContext)
    {
        _alertService = alertService;
        _hubContext = hubContext;
    }

    [HttpGet("UnreadCount")]
    public async Task<IActionResult> UnreadCount(
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var count = await _alertService.GetUnreadCountAsync(
            userId,
            cancellationToken);

        return Ok(new { count });
    }

    [HttpPost("MarkAllAsRead")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAsRead(
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        await _alertService.MarkAllAsReadAsync(
            userId,
            cancellationToken);

        await _hubContext.Clients
            .Group(WorkItemAlertHub.GetUserGroup(userId))
            .SendAsync(
                "UnreadCountChanged",
                0,
                cancellationToken);

        return Ok(new { count = 0 });
    }
}