using ChemResearchHub.Application.Bale.Models;

namespace ChemResearchHub.Application.Bale.Interfaces;

public interface IBaleAccountLinkingService
{
    Task<BaleLinkCodeResult> CreateLinkCodeAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<BaleLinkCodeResult> CompleteLinkAsync(
        string code,
        long chatId,
        bool isPrivateChat,
        CancellationToken cancellationToken = default);

    Task<bool> IsLinkedAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<bool> UnlinkAsync(
        string userId,
        CancellationToken cancellationToken = default);
}