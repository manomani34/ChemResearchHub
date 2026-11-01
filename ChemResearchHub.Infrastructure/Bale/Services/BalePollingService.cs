using ChemResearchHub.Application.Bale.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ChemResearchHub.Infrastructure.Bale.Configuration;
using Microsoft.Extensions.Options;

namespace ChemResearchHub.Infrastructure.Bale.Services;

public sealed class BalePollingService : BackgroundService
{
    private readonly IBaleBotClient _client;
    private readonly BaleBotOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BalePollingService> _logger;

    public BalePollingService(
        IBaleBotClient client,
        IOptions<BaleBotOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<BalePollingService> logger)
    {
        _client = client;
        _options = options.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        if (!_options.EnablePolling)
        {
            _logger.LogInformation("Bale polling is disabled.");
            return;
        }

        long? offset = null;

        _logger.LogInformation("Bale polling service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var updates = await _client.GetUpdatesAsync(
                    offset,
                    _options.PollingTimeoutSeconds,
                    stoppingToken);

                foreach (var update in updates)
                {
                    offset = update.UpdateId + 1;

                    var message = update.Message;

                    if (message?.Chat is null ||
                        string.IsNullOrWhiteSpace(message.Text))
                    {
                        continue;
                    }

                    await ProcessMessageAsync(
                        message.Chat.Id,
                        message.Chat.Type,
                        message.Text,
                        stoppingToken);
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Error while polling Bale updates.");

                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken);
            }
        }

        _logger.LogInformation("Bale polling service stopped.");
    }

    private async Task ProcessMessageAsync(
        long chatId,
        string? chatType,
        string text,
        CancellationToken cancellationToken)
    {
        var normalizedText = text.Trim();

        if (normalizedText.StartsWith("/start ", StringComparison.OrdinalIgnoreCase))
        {
            normalizedText = normalizedText["/start ".Length..].Trim();
        }

        if (normalizedText.StartsWith("/link ", StringComparison.OrdinalIgnoreCase))
        {
            normalizedText = normalizedText["/link ".Length..].Trim();
        }

        if (normalizedText.StartsWith("/start", StringComparison.OrdinalIgnoreCase) ||
            normalizedText.StartsWith("/link", StringComparison.OrdinalIgnoreCase))
        {
            await SendReplyAsync(
                chatId,
                "برای اتصال حساب، کد اتصال را بعد از دستور /link ارسال کنید.",
                cancellationToken);

            return;
        }

        if (!IsPossibleLinkCode(normalizedText))
        {
            await SendReplyAsync(
                chatId,
                "برای اتصال حساب ChemResearchHub، کد اتصال معتبر را ارسال کنید.",
                cancellationToken);

            return;
        }

        var isPrivateChat = string.Equals(
            chatType,
            "private",
            StringComparison.OrdinalIgnoreCase);

        using var scope = _scopeFactory.CreateScope();

        var linkingService = scope.ServiceProvider
            .GetRequiredService<IBaleAccountLinkingService>();

        var result = await linkingService.CompleteLinkAsync(
            normalizedText,
            chatId,
            isPrivateChat,
            cancellationToken);

        await SendReplyAsync(
            chatId,
            result.Succeeded
                ? "حساب ChemResearchHub شما با موفقیت به Bale متصل شد."
                : result.Error ?? "اتصال حساب انجام نشد.",
            cancellationToken);
    }

    private async Task SendReplyAsync(
        long chatId,
        string text,
        CancellationToken cancellationToken)
    {
        try
        {
            await _client.SendMessageAsync(
                chatId,
                text,
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to send Bale reply to chat {ChatId}.",
                chatId);
        }
    }

    private static bool IsPossibleLinkCode(string value)
    {
        return value.Length == 8 &&
               value.All(char.IsLetterOrDigit);
    }
}