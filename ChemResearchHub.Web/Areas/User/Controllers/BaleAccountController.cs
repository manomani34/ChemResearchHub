using System.Security.Claims;
using ChemResearchHub.Application.Bale.Interfaces;
using ChemResearchHub.Application.Bale.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChemResearchHub.Web.Areas.User.Controllers;

[Area("User")]
[Authorize]
[Route("User/BaleAccount")]
public sealed class BaleAccountController : Controller
{
    private readonly IBaleAccountLinkingService _linkingService;

    public BaleAccountController(
        IBaleAccountLinkingService linkingService)
    {
        _linkingService = linkingService;
    }

    [HttpGet("Status")]
    public async Task<IActionResult> Status(
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var isLinked = await _linkingService.IsLinkedAsync(
            userId,
            cancellationToken);

        return Ok(new
        {
            success = true,
            isLinked
        });
    }

    [HttpPost("CreateLinkCode")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLinkCode(
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _linkingService.CreateLinkCodeAsync(
            userId,
            cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                success = false,
                message = result.Error
            });
        }

        return Ok(new
        {
            success = true,
            code = result.Code,
            expiresAtUtc = result.ExpiresAtUtc,
            botUsername = "@chemresearchhub_bot",
            instructions = "ربات را در Bale باز کنید و کد را برای آن ارسال کنید."
        });
    }

    [HttpPost("Unlink")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlink(
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _linkingService.UnlinkAsync(
            userId,
            cancellationToken);

        if (!result)
        {
            return BadRequest(new
            {
                success = false,
                message = "قطع اتصال انجام نشد یا حساب متصل نیست."
            });
        }

        return Ok(new
        {
            success = true,
            message = "اتصال حساب Bale قطع شد."
        });
    }
}