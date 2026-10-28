
using ChemResearchHub.Application.Users.Dtos;
using ChemResearchHub.Application.Users.Interfaces;
using ChemResearchHub.Application.Users.Repositories;

namespace ChemResearchHub.Application.Users.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public Task<IReadOnlyList<UserDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _userRepository.GetAllAsync(cancellationToken);
    }

    public Task<IReadOnlyList<UserDto>> GetActiveUsersAsync(
        CancellationToken cancellationToken = default)
    {
        return _userRepository.GetActiveUsersAsync(cancellationToken);
    }

    public Task<UserDto?> GetByIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return _userRepository.GetByIdAsync(
            userId,
            cancellationToken);
    }

    public Task<bool> UpdateAsync(
        string userId,
        string fullName,
        string? email,
        IReadOnlyCollection<string> roleNames,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        return _userRepository.UpdateAsync(
            userId,
            fullName,
            email,
            roleNames,
            isActive,
            cancellationToken);
    }

    public Task<UserDto?> CreateAsync(
        string fullName,
        string email,
        string password,
        IReadOnlyCollection<string> roleNames,
        CancellationToken cancellationToken = default)
    {
        return _userRepository.CreateAsync(
            fullName,
            email,
            password,
            roleNames,
            cancellationToken);
    }
}
