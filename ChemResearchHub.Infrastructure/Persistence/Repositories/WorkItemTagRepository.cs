using ChemResearchHub.Application.Tags.Dtos;
using ChemResearchHub.Application.Tags.Repositories;
using ChemResearchHub.Domain.Entities.Tag;
using ChemResearchHub.Domain.Entities.WorkItemTag;
using ChemResearchHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChemResearchHub.Infrastructure.Persistence.Repositories;

public class WorkItemTagRepository : IWorkItemTagRepository
{
    private readonly ApplicationDbContext _context;

    public WorkItemTagRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyDictionary<int, IReadOnlyList<TagDto>>> GetTagsByWorkItemIdsAsync(
        IReadOnlyCollection<int> workItemIds,
        CancellationToken cancellationToken = default)
    {
        if (workItemIds.Count == 0)
            return new Dictionary<int, IReadOnlyList<TagDto>>();

        var rows = await (
            from relation in _context.WorkItemTags.AsNoTracking()
            join tag in _context.Tags.AsNoTracking()
                on relation.TagId equals tag.Id
            where workItemIds.Contains(relation.WorkItemId)
            orderby tag.Name
            select new
            {
                relation.WorkItemId,
                Tag = new TagDto
                {
                    Id = tag.Id,
                    Name = tag.Name,
                    Description = tag.Description,
                    Color = tag.Color,
                    IsActive = tag.IsActive
                }
            })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(x => x.WorkItemId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<TagDto>)group
                    .Select(x => x.Tag)
                    .ToList());
    }

    public async Task ReplaceActiveTagsAsync(
        int workItemId,
        IReadOnlyCollection<int> tagIds,
        CancellationToken cancellationToken = default)
    {
        var requestedIds = tagIds
            .Where(id => id > 0)
            .Distinct()
            .ToHashSet();

        if (requestedIds.Count != tagIds.Count(id => id > 0))
            throw new ArgumentException("Duplicate tag IDs are not allowed.", nameof(tagIds));

        var activeTags = await _context.Tags
            .Where(x => requestedIds.Contains(x.Id) && x.IsActive)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (activeTags.Count != requestedIds.Count)
            throw new ArgumentException(
                "One or more selected tags do not exist or are inactive.",
                nameof(tagIds));

        var existingRelations = await _context.WorkItemTags
            .Where(x => x.WorkItemId == workItemId)
            .ToListAsync(cancellationToken);

        var existingTagIds = existingRelations
            .Select(x => x.TagId)
            .ToHashSet();

        var existingActiveIds = await _context.Tags
            .Where(x => existingTagIds.Contains(x.Id) && x.IsActive)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var relationsToRemove = existingRelations
            .Where(x =>
                existingActiveIds.Contains(x.TagId) &&
                !requestedIds.Contains(x.TagId))
            .ToList();

        _context.WorkItemTags.RemoveRange(relationsToRemove);

        var relationsToAdd = requestedIds
            .Where(id => !existingTagIds.Contains(id))
            .Select(id => new WorkItemTag(workItemId, id))
            .ToList();

        await _context.WorkItemTags.AddRangeAsync(
            relationsToAdd,
            cancellationToken);
    }
}
