using ChemResearchHub.Application.Users.Dtos;
using ChemResearchHub.Application.Users.Interfaces;
using ChemResearchHub.Application.Users.Repositories;

namespace ChemResearchHub.Application.Users.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(
        IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetAllAsync(
            cancellationToken);
    }

    public async Task<IReadOnlyList<UserDto>> GetActiveUsersAsync(
        CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetActiveUsersAsync(
            cancellationToken);
    }

    public async Task<UserDto?> GetByIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetByIdAsync(
            userId,
            cancellationToken);
    }

    public async Task<bool> UpdateAsync(
        string userId,
        string fullName,
        string? email,
        string roleName,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        return await _userRepository.UpdateAsync(
            userId,
            fullName,
            email,
            roleName,
            isActive,
            cancellationToken);
    }

    public async Task<UserDto?> CreateAsync(
        string fullName,
        string email,
        string password,
        string roleName,
        CancellationToken cancellationToken = default)
    {
        return await _userRepository.CreateAsync(
            fullName,
            email,
            password,
            roleName,
            cancellationToken);
    }
}