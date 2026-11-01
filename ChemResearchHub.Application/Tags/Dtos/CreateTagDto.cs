namespace ChemResearchHub.Application.Tags.Dtos;

public class CreateTagDto
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string Color { get; init; } = "#64748B";
}
