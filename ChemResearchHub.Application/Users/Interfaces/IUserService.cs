using ChemResearchHub.Application.Users.Dtos;

namespace ChemResearchHub.Application.Users.Interfaces;

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserDto>> GetActiveUsersAsync(
        CancellationToken cancellationToken = default);

    Task<UserDto?> GetByIdAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        string userId,
        string fullName,
        string? email,
        IReadOnlyCollection<string> roleNames,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<UserDto?> CreateAsync(
        string fullName,
        string email,
        string password,
        IReadOnlyCollection<string> roleNames,
        CancellationToken cancellationToken = default);
}
