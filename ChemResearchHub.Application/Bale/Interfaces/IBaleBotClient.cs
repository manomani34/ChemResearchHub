using ChemResearchHub.Application.Bale.Models;

namespace ChemResearchHub.Application.Bale.Interfaces;

public interface IBaleBotClient
{
    Task<BaleUser?> GetMeAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BaleUpdate>> GetUpdatesAsync(
        long? offset = null,
        int timeout = 30,
        CancellationToken cancellationToken = default);

    Task<BaleMessage?> SendMessageAsync(
        long chatId,
        string text,
        CancellationToken cancellationToken = default);
}
