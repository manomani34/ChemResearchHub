using ChemResearchHub.Application.Tags.Dtos;
using ChemResearchHub.Application.Tags.Interfaces;
using ChemResearchHub.Application.Tags.Repositories;
using ChemResearchHub.Domain.Entities.Tag;

namespace ChemResearchHub.Application.Tags.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;

    public TagService(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<IReadOnlyList<TagDto>> GetAllAsync(bool? isActive = null,
                                                         CancellationToken cancellationToken = default)
    {
        var tags = await _tagRepository.GetAllAsync(
            isActive,
            cancellationToken);

        return tags.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<TagDto>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetAllAsync(true, cancellationToken);
    }

    public async Task<TagDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return null;

        var tag = await _tagRepository.GetByIdAsync(id, cancellationToken);

        return tag is null ? null : MapToDto(tag);
    }

    public async Task<TagDto> CreateAsync(
        CreateTagDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var name = NormalizeName(dto.Name);

        if (await _tagRepository.ExistsByNameAsync(
                name,
                cancellationToken: cancellationToken))
        {
            throw new ArgumentException(
                "A tag with this name already exists.",
                nameof(dto.Name));
        }

        var tag = new Tag(name, dto.Color, dto.Description);

        await _tagRepository.AddAsync(tag, cancellationToken);
        await _tagRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(tag);
    }

    public async Task<bool> UpdateAsync(int id,
                                        CreateTagDto dto,
                                        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return false;

        ArgumentNullException.ThrowIfNull(dto);

        var tag = await _tagRepository.GetByIdAsync(id, cancellationToken);

        if (tag is null)
            return false;

        var name = NormalizeName(dto.Name);

        if (await _tagRepository.ExistsByNameAsync(
                name,
                id,
                cancellationToken))
        {
            throw new ArgumentException(
                "A tag with this name already exists.",
                nameof(dto.Name));
        }

        tag.Update(name, dto.Color, dto.Description);

        await _tagRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ActivateAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return false;

        var tag = await _tagRepository.GetByIdAsync(id, cancellationToken);

        if (tag is null)
            return false;

        tag.Activate();

        await _tagRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeactivateAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return false;

        var tag = await _tagRepository.GetByIdAsync(id, cancellationToken);

        if (tag is null)
            return false;

        tag.Deactivate();

        await _tagRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static string NormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tag name is required.", nameof(name));

        return name.Trim();
    }

    private static TagDto MapToDto(Tag tag)
    {
        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Description = tag.Description,
            Color = tag.Color,
            IsActive = tag.IsActive
        };
    }
}
