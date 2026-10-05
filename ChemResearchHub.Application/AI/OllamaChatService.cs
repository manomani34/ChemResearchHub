using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChemResearchHub.Application.AI.Tools;

public class OllamaChatService : IAIChatService
{
    private readonly HttpClient _httpClient;
    private readonly AiToolRegistry _toolRegistry;

    public OllamaChatService(
        HttpClient httpClient,
        AiToolRegistry toolRegistry)
    {
        _httpClient = httpClient;
        _toolRegistry = toolRegistry;
    }

    public async Task<string> ChatAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        var messages = new List<OllamaMessage>
        {
            new()
            {
                Role = "system",
                Content =
                    """
                    You are the AI research assistant for ChemResearchHub.

                    Answer the user's question directly and clearly.

                    You have access to tools that provide real data
                    from the ChemResearchHub system.

                    IMPORTANT:
                    - Never invent database information.
                    - When the user's question requires information from
                      the ChemResearchHub database, use the appropriate tool.
                    - Always rely on tool results for database-related questions.
                    - Never assume or invent database values.
                    - If the user asks in Persian, answer in Persian.
                    - Return only the final answer.
                    - Do not output reasoning or internal thoughts.
                    """
            },

            new()
            {
                Role = "user",
                Content = message
            }
        };

        // تمام Toolهای ثبت شده در Registry
        var tools =
            _toolRegistry
                .GetDefinitions();

        // مرحله اول:
        // سؤال را به Qwen می‌فرستیم تا مشخص کند
        // آیا به Tool نیاز دارد یا خیر.
        var firstResponse =
            await SendToOllamaAsync(
                messages,
                tools,
                cancellationToken);

        // اگر Qwen هیچ Toolای درخواست نکرد،
        // پاسخ مستقیم خودش را برمی‌گردانیم.
        if (firstResponse.Message?.ToolCalls is null ||
            firstResponse.Message.ToolCalls.Count == 0)
        {
            return firstResponse.Message?.Content?.Trim() ?? "";
        }

        // پاسخ Qwen که شامل درخواست Tool است
        messages.Add(firstResponse.Message);

        // اجرای Toolهای درخواست شده
        foreach (var toolCall in firstResponse.Message.ToolCalls)
        {
            var toolName =
                toolCall.Function?.Name;

            if (string.IsNullOrWhiteSpace(toolName))
                continue;

            // پیدا کردن Tool از Registry
            var tool =
                _toolRegistry.GetTool(toolName);

            // اگر Tool وجود نداشت، از آن عبور می‌کنیم.
            if (tool is null)
                continue;

            // Arguments ارسال شده توسط Qwen
            var arguments =
                toolCall.Function.Arguments.GetRawText();

            // اجرای Tool
            var toolResult =
                await tool.ExecuteAsync(
                    arguments,
                    cancellationToken);

            // اضافه کردن نتیجه Tool به conversation
            messages.Add(new OllamaMessage
            {
                Role = "tool",
                Content = toolResult
            });
        }

        // مرحله دوم:
        // نتیجه Tool را دوباره به Qwen می‌دهیم
        // تا پاسخ نهایی قابل فهم برای کاربر تولید شود.
        var finalResponse =
            await SendToOllamaAsync(
                messages,
                tools: null,
                cancellationToken);

        return finalResponse.Message?.Content?.Trim() ?? "";
    }

    private async Task<OllamaResponse> SendToOllamaAsync(
        List<OllamaMessage> messages,
        object[]? tools,
        CancellationToken cancellationToken)
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

    private class OllamaResponse
    {
        [JsonPropertyName("message")]
        public OllamaMessage? Message { get; set; }
    }

    private class OllamaMessage
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("tool_calls")]
        public List<OllamaToolCall>? ToolCalls { get; set; }
    }

    private class OllamaToolCall
    {
        [JsonPropertyName("function")]
        public OllamaFunctionCall? Function { get; set; }
    }

    private class OllamaFunctionCall
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("arguments")]
        public JsonElement Arguments { get; set; }
    }
}