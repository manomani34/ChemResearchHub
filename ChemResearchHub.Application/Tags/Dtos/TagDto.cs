namespace ChemResearchHub.Application.Tags.Dtos;

public class TagDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string Color { get; init; } = null!;
    public bool IsActive { get; init; }
}
