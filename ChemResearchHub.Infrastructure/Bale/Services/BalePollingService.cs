using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ChemResearchHub.Application.Bale.Interfaces;
using ChemResearchHub.Infrastructure.Bale.Configuration;
using Microsoft.Extensions.Options;

namespace ChemResearchHub.Infrastructure.Bale.Services;

public sealed class BalePollingService : BackgroundService
{
    private readonly IBaleBotClient _client;
    private readonly BaleBotOptions _options;
    private readonly ILogger<BalePollingService> _logger;

    public BalePollingService(
        IBaleBotClient client,
        IOptions<BaleBotOptions> options,
        ILogger<BalePollingService> logger)
    {
        _client = client;
        _options = options.Value;
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

                    if (message?.Chat is null)
                        continue;

                    _logger.LogInformation(
                        "Bale message received. ChatId: {ChatId}, ChatType: {ChatType}, UserId: {UserId}, Text: {Text}",
                        message.Chat.Id,
                        message.Chat.Type,
                        message.From?.Id,
                        message.Text);
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
}

