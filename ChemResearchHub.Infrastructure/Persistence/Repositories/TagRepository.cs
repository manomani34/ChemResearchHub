using ChemResearchHub.Application.Tags.Repositories;
using ChemResearchHub.Domain.Entities.Tag;
using ChemResearchHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChemResearchHub.Infrastructure.Persistence.Repositories;

public class TagRepository : ITagRepository
{
    private readonly ApplicationDbContext _context;

    public TagRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Tag>> GetAllAsync(
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Tags.AsNoTracking();

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        return await query
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<Tag?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return _context.Tags
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(
        string name,
        int? excludedId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Tags
            .AsNoTracking()
            .Where(x => x.Name == name);

        if (excludedId.HasValue)
            query = query.Where(x => x.Id != excludedId.Value);

        return query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(
        Tag tag,
        CancellationToken cancellationToken = default)
    {
        await _context.Tags.AddAsync(tag, cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
