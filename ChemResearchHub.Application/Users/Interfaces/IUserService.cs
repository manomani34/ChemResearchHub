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
        string roleName,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<UserDto?> CreateAsync(
        string fullName,
        string email,
        string password,
        string roleName,
        CancellationToken cancellationToken = default);
}