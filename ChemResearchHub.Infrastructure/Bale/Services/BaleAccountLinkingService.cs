using System.Security.Cryptography;
using ChemResearchHub.Application.Bale.Interfaces;
using ChemResearchHub.Application.Bale.Models;
using ChemResearchHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ChemResearchHub.Infrastructure.Bale.Services;

public sealed class BaleAccountLinkingService
    : IBaleAccountLinkingService
{
    private const string CodeCachePrefix = "bale-link-code:";
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(5);

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<BaleAccountLinkingService> _logger;

    public BaleAccountLinkingService(
        UserManager<ApplicationUser> userManager,
        IMemoryCache memoryCache,
        ILogger<BaleAccountLinkingService> logger)
    {
        _userManager = userManager;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<BaleLinkCodeResult> CreateLinkCodeAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BaleLinkCodeResult.Failure("شناسه کاربر معتبر نیست.");

        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return BaleLinkCodeResult.Failure("کاربر پیدا نشد.");

        if (user.BaleChatId.HasValue)
            return BaleLinkCodeResult.Failure(
                "حساب Bale شما قبلاً متصل شده است.");

        var code = GenerateCode();

        var cacheKey = CodeCachePrefix + code;
        var expiresAt = DateTime.UtcNow.Add(CodeLifetime);

        _memoryCache.Set(
            cacheKey,
            userId,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = expiresAt
            });

        return BaleLinkCodeResult.Success(code, expiresAt);
    }

    public async Task<BaleLinkCodeResult> CompleteLinkAsync(
        string code,
        long chatId,
        bool isPrivateChat,
        CancellationToken cancellationToken = default)
    {
        if (!isPrivateChat)
            return BaleLinkCodeResult.Failure(
                "اتصال حساب فقط از طریق گفت‌وگوی خصوصی با ربات امکان‌پذیر است.");

        if (string.IsNullOrWhiteSpace(code))
            return BaleLinkCodeResult.Failure("کد اتصال ارسال نشده است.");

        if (chatId <= 0)
            return BaleLinkCodeResult.Failure("شناسه گفت‌وگوی Bale معتبر نیست.");

        cancellationToken.ThrowIfCancellationRequested();

        code = code.Trim().ToUpperInvariant();

        var cacheKey = CodeCachePrefix + code;

        if (!_memoryCache.TryGetValue(cacheKey, out string? userId) ||
            string.IsNullOrWhiteSpace(userId))
        {
            return BaleLinkCodeResult.Failure(
                "کد اتصال نامعتبر یا منقضی شده است.");
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            _memoryCache.Remove(cacheKey);

            return BaleLinkCodeResult.Failure(
                "حساب کاربری موردنظر پیدا نشد.");
        }

        if (user.BaleChatId.HasValue)
        {
            _memoryCache.Remove(cacheKey);

            return BaleLinkCodeResult.Failure(
                "این حساب قبلاً به Bale متصل شده است.");
        }

        var existingUser = await _userManager.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.BaleChatId == chatId,
                cancellationToken);

        if (existingUser is not null)
        {
            _memoryCache.Remove(cacheKey);

            return BaleLinkCodeResult.Failure(
                "این حساب Bale قبلاً به یک کاربر دیگر متصل شده است.");
        }

        user.BaleChatId = chatId;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            _logger.LogError(
                "Failed to link Bale account for user {UserId}. Errors: {Errors}",
                user.Id,
                string.Join(
                    "; ",
                    updateResult.Errors.Select(x => x.Description)));

            return BaleLinkCodeResult.Failure(
                "ذخیره اتصال حساب انجام نشد. لطفاً دوباره تلاش کنید.");
        }

        _memoryCache.Remove(cacheKey);

        _logger.LogInformation(
            "Bale account successfully linked for user {UserId}.",
            user.Id);

        return BaleLinkCodeResult.Success(
            "حساب Bale با موفقیت متصل شد.",
            DateTime.UtcNow);
    }

    public async Task<bool> IsLinkedAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.BaleChatId.HasValue == true;
    }

    public async Task<bool> UnlinkAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null || !user.BaleChatId.HasValue)
            return false;

        user.BaleChatId = null;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            _logger.LogError(
                "Failed to unlink Bale account for user {UserId}. Errors: {Errors}",
                userId,
                string.Join(
                    "; ",
                    result.Errors.Select(x => x.Description)));

            return false;
        }

        return true;
    }

    private static string GenerateCode()
    {
        const string characters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        var bytes = RandomNumberGenerator.GetBytes(8);

        return string.Concat(
            bytes.Select(x => characters[x % characters.Length]));
    }
}