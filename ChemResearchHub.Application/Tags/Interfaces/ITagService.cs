using ChemResearchHub.Application.Tags.Dtos;

namespace ChemResearchHub.Application.Tags.Interfaces;

public interface ITagService
{
    Task<IReadOnlyList<TagDto>> GetAllAsync(
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TagDto>> GetActiveAsync(
        CancellationToken cancellationToken = default);

    Task<TagDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<TagDto> CreateAsync(
        CreateTagDto dto,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        int id,
        CreateTagDto dto,
        CancellationToken cancellationToken = default);

    Task<bool> ActivateAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> DeactivateAsync(
        int id,
        CancellationToken cancellationToken = default);
}
