using ChemResearchHub.Application.Boards.Repositories;
using ChemResearchHub.Domain.Entities.Board;
using Microsoft.EntityFrameworkCore;

namespace ChemResearchHub.Infrastructure.Persistence.Repositories;

public class BoardRepository : IBoardRepository
{
    private readonly ApplicationDbContext _dbContext;

    public BoardRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Board>> GetByProjectIdAsync(
        int projectId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Boards
            .AsNoTracking()
            .Include(x => x.Columns)
            .Where(x => x.ProjectId == projectId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Board?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Boards
            .Include(x => x.Columns)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<Board?> GetByColumnIdAsync(
        int boardColumnId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Boards
            .Include(x => x.Columns)
            .FirstOrDefaultAsync(
                x => x.Columns.Any(
                    column => column.Id == boardColumnId),
                cancellationToken);
    }

    public async Task AddAsync(
        Board board,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Boards.AddAsync(
            board,
            cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}