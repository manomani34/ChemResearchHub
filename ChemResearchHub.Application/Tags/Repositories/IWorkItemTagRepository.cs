using ChemResearchHub.Application.Tags.Dtos;

namespace ChemResearchHub.Application.Tags.Repositories;

public interface IWorkItemTagRepository
{
    Task<IReadOnlyDictionary<int, IReadOnlyList<TagDto>>> GetTagsByWorkItemIdsAsync(
        IReadOnlyCollection<int> workItemIds,
        CancellationToken cancellationToken = default);

    Task ReplaceActiveTagsAsync(
        int workItemId,
        IReadOnlyCollection<int> tagIds,
        CancellationToken cancellationToken = default);
}
