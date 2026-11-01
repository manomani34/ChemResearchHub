using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ChemResearchHub.Web.Hubs;

[Authorize]
public class WorkItemAlertHub : Hub
{
    public const string HubUrl = "/hubs/work-item-alerts";

    public static string GetUserGroup(string userId)
        => $"work-item-alerts:{userId}";

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                GetUserGroup(userId));
        }

        await base.OnConnectedAsync();
    }
}