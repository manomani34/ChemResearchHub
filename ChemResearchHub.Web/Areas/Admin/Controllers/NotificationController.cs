using ChemResearchHub.Application.Notifications.Interfaces;
using ChemResearchHub.Application.Notifications.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChemResearchHub.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
[Route("Admin/Notifications")]
public class NotificationController : Controller
{
    private readonly INotificationService _notificationService;

    public NotificationController(
        INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost("send-test")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendTest(
        [FromForm] string userId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(new
            {
                success = false,
                message = "شناسه کاربر الزامی است."
            });

        var result = await _notificationService.SendAsync(
            new NotificationMessage
            {
                UserId = userId,
                Type = "Test",
                Title = "اعلان آزمایشی ChemResearchHub",
                Message = "این یک اعلان آزمایشی از سامانه است."
            },
            cancellationToken);

        return Ok(new
        {
            success = result.Channels.Any(x => x.Succeeded),
            channels = result.Channels
        });
    }
}