using ChemResearchHub.Domain.Common;

namespace ChemResearchHub.Domain.Entities.Board;

public class BoardColumn : BaseEntity
{
    private BoardColumn()
    {
    }

    internal BoardColumn(
        Board board,
        string name,
        int order,
        int? wipLimit = null)
    {
        ArgumentNullException.ThrowIfNull(board);

        SetName(name);

        if (order < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(order),
                "Column order cannot be negative.");
        }

        if (wipLimit.HasValue && wipLimit.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(wipLimit),
                "WIP limit must be greater than zero.");
        }

        Board = board;
        BoardId = board.Id;
        Order = order;
        WipLimit = wipLimit;
    }

    public int BoardId { get; private set; }

    public Board Board { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public int Order { get; private set; }

    public int? WipLimit { get; private set; }

    public void Rename(string name)
    {
        SetName(name);
        ModifiedAt = DateTime.UtcNow;
    }

    public void ChangeOrder(int order)
    {
        if (order < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(order),
                "Column order cannot be negative.");
        }

        Order = order;
        ModifiedAt = DateTime.UtcNow;
    }

    public void SetWipLimit(int? wipLimit)
    {
        if (wipLimit.HasValue && wipLimit.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(wipLimit),
                "WIP limit must be greater than zero.");
        }

        WipLimit = wipLimit;
        ModifiedAt = DateTime.UtcNow;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Column name is required.",
                nameof(name));
        }

        Name = name.Trim();
    }
}