using ChemResearchHub.Domain.Common;

namespace ChemResearchHub.Domain.Entities.Board;

public class Board : BaseEntity
{
    private readonly List<BoardColumn> _columns = new();

    private Board()
    {
    }

    public Board(
        int projectId,
        string name)
    {
        if (projectId <= 0)
        {
            throw new ArgumentException(
                "ProjectId must be greater than zero.",
                nameof(projectId));
        }

        SetName(name);

        ProjectId = projectId;
    }

    public int ProjectId { get; private set; }

    public string Name { get; private set; } = null!;

    public IReadOnlyCollection<BoardColumn> Columns =>
        _columns.AsReadOnly();

    public void Rename(string name)
    {
        SetName(name);
        ModifiedAt = DateTime.UtcNow;
    }

    public BoardColumn AddColumn(
    string name,
    int order,
    int? wipLimit = null)
    {
        var column = new BoardColumn(
            this,
            name,
            order,
            wipLimit);

        _columns.Add(column);

        ModifiedAt = DateTime.UtcNow;

        return column;
    }

    public void RemoveColumn(int columnId)
    {
        var column = _columns.FirstOrDefault(
            x => x.Id == columnId);

        if (column is null)
        {
            return;
        }

        _columns.Remove(column);

        ModifiedAt = DateTime.UtcNow;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Board name is required.",
                nameof(name));
        }

        Name = name.Trim();
    }
}