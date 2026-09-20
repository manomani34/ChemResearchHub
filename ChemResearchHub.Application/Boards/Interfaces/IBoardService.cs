using ChemResearchHub.Application.Boards.Dtos;

namespace ChemResearchHub.Application.Boards.Interfaces;

public interface IBoardService
{
    Task<IReadOnlyList<BoardDto>> GetByProjectIdAsync(
        int projectId,
        CancellationToken cancellationToken = default);

    Task<BoardDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<BoardDto> CreateAsync(
        int projectId,
        string name,
        CancellationToken cancellationToken = default);
}