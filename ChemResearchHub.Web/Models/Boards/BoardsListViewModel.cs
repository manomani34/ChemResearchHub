namespace ChemResearchHub.Web.Models.Boards;

public class BoardsListViewModel
{
    public IReadOnlyList<BoardListItemViewModel> Items { get; init; }
        = Array.Empty<BoardListItemViewModel>();
}

public class BoardListItemViewModel
{
    public int Id { get; init; }

    public int ProjectId { get; init; }

    public string ProjectName { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public int ColumnCount { get; init; }
}