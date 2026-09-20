using ChemResearchHub.Domain.Entities.Board;

namespace ChemResearchHub.Application.Boards.Repositories;

public interface IBoardRepository
{
    Task<IReadOnlyList<Board>> GetByProjectIdAsync(
        int projectId,
        CancellationToken cancellationToken = default);

    Task<Board?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Board?> GetByColumnIdAsync(
        int boardColumnId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Board board,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}