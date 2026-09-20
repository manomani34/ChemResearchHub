namespace ChemResearchHub.Application.Boards.Dtos;

public class BoardDto
{
    public int Id { get; init; }

    public int ProjectId { get; init; }

    public string Name { get; init; } = null!;

    public IReadOnlyList<BoardColumnDto> Columns { get; init; }
        = Array.Empty<BoardColumnDto>();
}

public class BoardColumnDto
{
    public int Id { get; init; }

    public string Name { get; init; } = null!;

    public int Order { get; init; }

    public int? WipLimit { get; init; }
}