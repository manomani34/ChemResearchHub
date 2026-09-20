using ChemResearchHub.Application.Boards.Dtos;
using ChemResearchHub.Application.Boards.Interfaces;
using ChemResearchHub.Application.Boards.Repositories;
using ChemResearchHub.Domain.Entities.Board;

namespace ChemResearchHub.Application.Boards.Services;

public class BoardService : IBoardService
{
    private readonly IBoardRepository _boardRepository;

    public BoardService(
        IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<IReadOnlyList<BoardDto>> GetByProjectIdAsync(
        int projectId,
        CancellationToken cancellationToken = default)
    {
        var boards =
            await _boardRepository.GetByProjectIdAsync(
                projectId,
                cancellationToken);

        return boards
            .Select(MapToDto)
            .ToList();
    }

    public async Task<BoardDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var board =
            await _boardRepository.GetByIdAsync(
                id,
                cancellationToken);

        return board is null
            ? null
            : MapToDto(board);
    }

    public async Task<BoardDto> CreateAsync(
        int projectId,
        string name,
        CancellationToken cancellationToken = default)
    {
        var board = new Board(
            projectId,
            name);

        AddDefaultColumns(board);

        await _boardRepository.AddAsync(
            board,
            cancellationToken);

        await _boardRepository.SaveChangesAsync(
            cancellationToken);

        return MapToDto(board);
    }

    private static void AddDefaultColumns(
        Board board)
    {
        board.AddColumn(
            "Research Backlog",
            0);

        board.AddColumn(
            "Ready",
            1);

        board.AddColumn(
            "In Progress",
            2);

        board.AddColumn(
            "Review / Validation",
            3);

        board.AddColumn(
            "Done",
            4);
    }

    private static BoardDto MapToDto(
        Board board)
    {
        return new BoardDto
        {
            Id = board.Id,
            ProjectId = board.ProjectId,
            Name = board.Name,
            Columns = board.Columns
                .OrderBy(x => x.Order)
                .Select(x => new BoardColumnDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Order = x.Order,
                    WipLimit = x.WipLimit
                })
                .ToList()
        };
    }
}