using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChemResearchHub.Application.AI.Ollama;

public class OllamaClient : IOllamaClient
{
    private readonly HttpClient _httpClient;

    public OllamaClient(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<OllamaResponse> ChatAsync(
        List<OllamaMessage> messages,
        object[]? tools,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            model = "qwen3:4b-instruct-2507-q4_K_M",
            messages,
            tools,
            stream = false,
            options = new
            {
                temperature = 0.1,
                num_predict = 500
            }
        };

        var response =
            await _httpClient.PostAsJsonAsync(
                "/api/chat",
                request,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content
                .ReadFromJsonAsync<OllamaResponse>(
                    cancellationToken: cancellationToken);

        return result ?? new OllamaResponse();
    }
}

public class OllamaResponse
{
    [JsonPropertyName("message")]
    public OllamaMessage? Message { get; set; }
}

public class OllamaMessage
{
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("tool_calls")]
    public List<OllamaToolCall>? ToolCalls { get; set; }
}

public class OllamaToolCall
{
    [JsonPropertyName("function")]
    public OllamaFunctionCall? Function { get; set; }
}

public class OllamaFunctionCall
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("arguments")]
    public JsonElement Arguments { get; set; }
}