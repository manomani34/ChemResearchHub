using ChemResearchHub.Application.AI.Tools;
using System.Text;
using System.Text.Json;

namespace ChemResearchHub.Application.AI.Sample;

public class SampleRequestHandler
{
    private readonly AiToolRegistry _toolRegistry;

    public SampleRequestHandler(
        AiToolRegistry toolRegistry)
    {
        _toolRegistry = toolRegistry;
    }

    public async Task<string> HandleAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        if (IsSpecificSampleRequest(message))
        {
            return await HandleSpecificSampleAsync(
                message,
                cancellationToken);
        }

        var experimentTitle =
            ExtractExperimentTitle(message);

        if (string.IsNullOrWhiteSpace(experimentTitle))
        {
            return "لطفاً عنوان آزمایش یا کد نمونه را مشخص کنید.";
        }

        var tool =
            _toolRegistry.GetTool(
                "get_samples_by_experiment");

        if (tool is null)
        {
            return "ابزار دریافت نمونه های آزمایش در دسترس نیست.";
        }

        var arguments =
            JsonSerializer.Serialize(new
            {
                experimentTitle
            });

        var result =
            await tool.ExecuteAsync(
                arguments,
                cancellationToken);

        return FormatSamplesResult(
            result,
            experimentTitle);
    }

    private async Task<string> HandleSpecificSampleAsync(
        string message,
        CancellationToken cancellationToken)
    {
        var sampleCode =
            ExtractSampleCode(message);

        if (string.IsNullOrWhiteSpace(sampleCode))
        {
            return "لطفاً کد نمونه را مشخص کنید.";
        }

        var tool =
            _toolRegistry.GetTool("get_sample");

        if (tool is null)
        {
            return "ابزار دریافت اطلاعات نمونه در دسترس نیست.";
        }

        var arguments =
            JsonSerializer.Serialize(new
            {
                sampleCode
            });

        var result =
            await tool.ExecuteAsync(
                arguments,
                cancellationToken);

        return FormatSampleResult(
            result,
            sampleCode);
    }

    // =========================================================
    // DETECTION
    // =========================================================

    private static bool IsSpecificSampleRequest(
        string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return false;

        var text =
            message.Trim().ToLowerInvariant();

        return
            text.Contains("اطلاعات نمونه") ||
            text.Contains("اطلاعات یک نمونه") ||
            text.Contains("جزئیات نمونه") ||
            text.Contains("جزئیات یک نمونه") ||
            text.Contains("درباره نمونه") ||
            text.Contains("درباره یک نمونه") ||
            text.Contains("sample details") ||
            text.Contains("sample info") ||
            text.Contains("information about sample");
    }

    // =========================================================
    // EXTRACTION
    // =========================================================

    private static string? ExtractSampleCode(
        string message)
    {
        var markers = new[]
        {
            "اطلاعات یک نمونه",
            "اطلاعات نمونه",
            "جزئیات یک نمونه",
            "جزئیات نمونه",
            "درباره یک نمونه",
            "درباره نمونه",
            "sample details",
            "sample info",
            "information about sample"
        };

        return ExtractAfterMarker(
            message,
            markers);
    }

    private static string? ExtractExperimentTitle(
        string message)
    {
        var markers = new[]
        {
            "نمونه های مربوط به آزمایش",
            "نمونه‌های مربوط به آزمایش",
            "نمونه های آزمایش",
            "نمونه‌های آزمایش",
            "نمونه آزمایش",
            "samples of experiment",
            "samples from experiment",
            "samples in experiment"
        };

        return ExtractAfterMarker(
            message,
            markers);
    }

    private static string? ExtractAfterMarker(
        string message,
        string[] markers)
    {
        if (string.IsNullOrWhiteSpace(message))
            return null;

        var text =
            message.Trim();

        foreach (var marker in markers)
        {
            var index =
                text.IndexOf(
                    marker,
                    StringComparison.OrdinalIgnoreCase);

            if (index < 0)
                continue;

            var value =
                text[(index + marker.Length)..]
                    .Trim();

            value =
                RemoveRequestEnding(value);

            value =
                value.Trim(
                    '«',
                    '»',
                    '"',
                    '\'',
                    '؟',
                    '?',
                    '.',
                    ' ');

            return string.IsNullOrWhiteSpace(value)
                ? null
                : value;
        }

        return null;
    }

    private static string RemoveRequestEnding(
        string text)
    {
        var endings = new[]
        {
            "رو پیدا کن",
            "را پیدا کن",
            "رو پیدا کنید",
            "را پیدا کنید",

            "رو پیدا کن لطفا",
            "را پیدا کن لطفا",
            "رو پیدا کن لطفاً",
            "را پیدا کن لطفاً",

            "رو نشون بده",
            "را نشون بده",
            "رو نشان بده",
            "را نشان بده",

            "رو نمایش بده",
            "را نمایش بده",

            "رو بیار",
            "را بیار",

            "رو بده",
            "را بده",

            "رو بگو",
            "را بگو"
        };

        foreach (var ending in endings)
        {
            if (text.EndsWith(
                    ending,
                    StringComparison.OrdinalIgnoreCase))
            {
                return text[..^ending.Length].Trim();
            }
        }

        return text
            .Trim()
            .Trim('؟', '?', '.', ' ');
    }

    // =========================================================
    // FORMAT - SPECIFIC SAMPLE
    // =========================================================

    private static string FormatSampleResult(
        string toolResult,
        string sampleCode)
    {
        try
        {
            using var document =
                JsonDocument.Parse(toolResult);

            var root =
                document.RootElement;

            if (root.TryGetProperty(
                    "found",
                    out var found) &&
                found.ValueKind == JsonValueKind.False)
            {
                return $"نمونه «{sampleCode}» پیدا نشد.";
            }

            if (root.ValueKind == JsonValueKind.Null)
            {
                return $"نمونه «{sampleCode}» پیدا نشد.";
            }

            var response =
                new StringBuilder();

            var actualCode =
                root.TryGetProperty(
                    "sampleCode",
                    out var code)
                    ? code.GetString()
                    : sampleCode;

            response.AppendLine(
                $"اطلاعات نمونه «{actualCode}»:");

            AppendValue(
                root,
                "id",
                "شناسه",
                response);

            AppendValue(
                root,
                "experimentId",
                "شناسه آزمایش",
                response);

            AppendValue(
                root,
                "name",
                "نام",
                response);

            AppendValue(
                root,
                "sampleType",
                "نوع نمونه",
                response);

            AppendValue(
                root,
                "matrix",
                "ماتریس",
                response);

            AppendValue(
                root,
                "preparationMethod",
                "روش آماده سازی",
                response);

            AppendValue(
                root,
                "collectedAt",
                "تاریخ دریافت",
                response);

            AppendValue(
                root,
                "externalReference",
                "مرجع خارجی",
                response);

            AppendValue(
                root,
                "description",
                "توضیحات",
                response);

            AppendValue(
                root,
                "notes",
                "یادداشت",
                response);

            return response
                .ToString()
                .Trim();
        }
        catch (JsonException)
        {
            return "در پردازش اطلاعات نمونه مشکلی پیش آمد.";
        }
    }

    private static void AppendValue(
        JsonElement root,
        string propertyName,
        string displayName,
        StringBuilder response)
    {
        if (!root.TryGetProperty(
                propertyName,
                out var element))
        {
            return;
        }

        if (element.ValueKind == JsonValueKind.Null)
            return;

        var value =
            element.ToString();

        if (string.IsNullOrWhiteSpace(value))
            return;

        response.AppendLine(
            $"{displayName}: {value}");
    }

    // =========================================================
    // FORMAT - SAMPLE LIST
    // =========================================================

    private static string FormatSamplesResult(
        string toolResult,
        string experimentTitle)
    {
        try
        {
            using var document =
                JsonDocument.Parse(toolResult);

            var root =
                document.RootElement;

            var data =
                root.TryGetProperty(
                    "result",
                    out var resultElement)
                    ? resultElement
                    : root;

            if (data.ValueKind == JsonValueKind.Null)
            {
                return
                    $"آزمایش «{experimentTitle}» پیدا نشد.";
            }

            if (data.TryGetProperty(
                    "found",
                    out var found) &&
                found.ValueKind == JsonValueKind.False)
            {
                return
                    $"آزمایش «{experimentTitle}» پیدا نشد.";
            }

            var actualTitle =
                data.TryGetProperty(
                    "experimentTitle",
                    out var title)
                    ? title.GetString()
                    : experimentTitle;

            var count =
                data.TryGetProperty(
                    "count",
                    out var countElement)
                    ? countElement.GetInt32()
                    : 0;

            if (count == 0)
            {
                return
                    $"برای آزمایش «{actualTitle}» نمونه ای ثبت نشده است.";
            }

            if (!data.TryGetProperty(
                    "samples",
                    out var samples))
            {
                return
                    $"تعداد {count} نمونه برای آزمایش «{actualTitle}» ثبت شده است.";
            }

            var response =
                new StringBuilder();

            response.AppendLine(
                $"نمونه های آزمایش «{actualTitle}»:");

            response.AppendLine(
                $"تعداد: {count}");

            response.AppendLine();

            var index = 1;

            foreach (var sample in
                     samples.EnumerateArray())
            {
                var code =
                    sample.TryGetProperty(
                        "sampleCode",
                        out var codeElement)
                        ? codeElement.GetString()
                        : null;

                var name =
                    sample.TryGetProperty(
                        "name",
                        out var nameElement)
                        ? nameElement.GetString()
                        : null;

                response.AppendLine(
                    $"{index}. کد نمونه: {code ?? "نامشخص"}");

                if (!string.IsNullOrWhiteSpace(name))
                {
                    response.AppendLine(
                        $"   نام: {name}");
                }

                index++;
            }

            return response
                .ToString()
                .Trim();
        }
        catch (JsonException)
        {
            return "در پردازش اطلاعات نمونه ها مشکلی پیش آمد.";
        }
    }
}