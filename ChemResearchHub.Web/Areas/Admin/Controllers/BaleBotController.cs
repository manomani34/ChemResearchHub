using ChemResearchHub.Application.Bale.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChemResearchHub.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
[Route("Admin/BaleBot")]
public class BaleBotController : Controller
{
    private readonly IBaleBotClient _baleBotClient;

    public BaleBotController(IBaleBotClient baleBotClient)
    {
        _baleBotClient = baleBotClient;
    }

    [HttpGet("GetMe")]
    public async Task<IActionResult> GetMe(
        CancellationToken cancellationToken)
    {
        try
        {
            var bot = await _baleBotClient.GetMeAsync(cancellationToken);

            return Json(new
            {
                success = true,
                bot
            });
        }
        catch (Exception exception)
        {
            return StatusCode(502, new
            {
                success = false,
                message = exception.Message
            });
        }
    }

    [HttpPost("SendTestMessage")]
    public async Task<IActionResult> SendTestMessage(
        [FromForm] long chatId,
        [FromForm] string text,
        CancellationToken cancellationToken)
    {
        try
        {
            var message = await _baleBotClient.SendMessageAsync(
                chatId, text, cancellationToken);

            return Json(new
            {
                success = true,
                message
            });
        }
        catch (Exception exception)
        {
            return StatusCode(502, new
            {
                success = false,
                message = exception.Message
            });
        }
    }
}
