using ChemResearchHub.Application.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChemResearchHub.Web.Areas.AI.Controllers;

[Area("AI")]
[ApiController]
[Route("api/[area]/[controller]")]
[AllowAnonymous]
public class AiController : ControllerBase
{
    private readonly IAIChatService _aiChatService;

    private readonly ProjectAiContext _projectAiContext;

    public AiController(
        IAIChatService aiChatService,
        ProjectAiContext projectAiContext)
    {
        _aiChatService = aiChatService;
        _projectAiContext = projectAiContext;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask(
        [FromBody] AiChatRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("پیام خالی است.");

        var response = await _aiChatService.ChatAsync(
            request.Message,
            cancellationToken);

        return Ok(new
        {
            answer = response
        });
    }

    [HttpGet("projects")]
    public async Task<IActionResult> GetProjects(
    CancellationToken cancellationToken)
    {
        var result =
            await _projectAiContext.GetProjectsAsync(
                cancellationToken);

        return Ok(result);
    }
}

public class AiChatRequest
{
    public string Message { get; set; } = string.Empty;
}