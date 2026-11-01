using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using ChemResearchHub.Application.Bale.Interfaces;
using ChemResearchHub.Infrastructure.Bale.Configuration;
using ChemResearchHub.Infrastructure.Bale.Models;
using ChemResearchHub.Application.Bale.Models;
using Microsoft.Extensions.Options;

namespace ChemResearchHub.Infrastructure.Bale.Clients;

public sealed class BaleBotClient : IBaleBotClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BaleBotClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public BaleBotClient(
        HttpClient httpClient,
        IOptions<BaleBotOptions> options,
        ILogger<BaleBotClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var settings = options.Value;

        if (string.IsNullOrWhiteSpace(settings.Token))
            throw new InvalidOperationException("Bale bot token is not configured.");

        var baseUrl = settings.ApiBaseUrl.TrimEnd('/');
        _httpClient.BaseAddress =
            new Uri($"{baseUrl}/bot{settings.Token}/");

        _httpClient.Timeout = TimeSpan.FromSeconds(60);
    }

    public async Task<BaleUser?> GetMeAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await GetAsync<BaleUser>(
            "getMe", cancellationToken);

        return response;
    }

    public async Task<IReadOnlyList<BaleUpdate>> GetUpdatesAsync(
        long? offset = null,
        int timeout = 30,
        CancellationToken cancellationToken = default)
    {
        var query = $"getUpdates?timeout={Math.Clamp(timeout, 1, 50)}";

        if (offset.HasValue)
            query += $"&offset={offset.Value}";

        return await GetAsync<List<BaleUpdate>>(
                   query, cancellationToken)
               ?? [];
    }

    public async Task<BaleMessage?> SendMessageAsync(
        long chatId,
        string text,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Message text cannot be empty.", nameof(text));

        var response = await _httpClient.PostAsJsonAsync(
            "sendMessage",
            new
            {
                chat_id = chatId,
                text
            },
            cancellationToken);

        return await ReadResponseAsync<BaleMessage>(
            response, cancellationToken);
    }

    private async Task<T?> GetAsync<T>(
        string endpoint,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(
            endpoint, cancellationToken);

        return await ReadResponseAsync<T>(
            response, cancellationToken);
    }

    private async Task<T?> ReadResponseAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Bale API HTTP error. Status: {StatusCode}, Response: {Response}",
                (int)response.StatusCode,
                body);

            throw new HttpRequestException(
                $"Bale API returned HTTP {(int)response.StatusCode}: {body}");
        }

        var result = JsonSerializer.Deserialize<BaleApiResponse<T>>(
            body, JsonOptions);

        if (result is null)
            throw new InvalidOperationException("Invalid response received from Bale API.");

        if (!result.Ok)
        {
            _logger.LogError(
                "Bale API error. Code: {ErrorCode}, Description: {Description}",
                result.ErrorCode,
                result.Description);

            throw new InvalidOperationException(
                $"Bale API error: {result.Description ?? "Unknown error"}");
        }

        return result.Result;
    }
}


