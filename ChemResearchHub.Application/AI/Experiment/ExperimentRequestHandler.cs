using ChemResearchHub.Application.AI.Tools;
using System.Text;
using System.Text.Json;

namespace ChemResearchHub.Application.AI.Experiment;

public class ExperimentRequestHandler
{
    private readonly AiToolRegistry _toolRegistry;

    public ExperimentRequestHandler(
        AiToolRegistry toolRegistry)
    {
        _toolRegistry = toolRegistry;
    }

    public async Task<string> HandleAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        if (IsLatestRequest(message))
        {
            return await HandleLatestAsync(
                cancellationToken);
        }

        if (IsStatisticsRequest(message))
        {
            return await HandleStatisticsAsync(
                message,
                cancellationToken);
        }

        if (IsProjectRequest(message))
        {
            return await HandleProjectAsync(
                message,
                cancellationToken);
        }

        if (IsSpecificRequest(message))
        {
            return await HandleSpecificAsync(
                message,
                cancellationToken);
        }

        return await HandleSearchAsync(
            message,
            cancellationToken);
    }

    // =========================================================
    // LATEST
    // =========================================================

    private async Task<string> HandleLatestAsync(
        CancellationToken cancellationToken)
    {
        var tool =
            _toolRegistry.GetTool(
                "get_latest_experiment");

        if (tool is null)
            return "ابزار دریافت آخرین آزمایش در دسترس نیست.";

        var result =
            await tool.ExecuteAsync(
                "{}",
                cancellationToken);

        return FormatLatestResult(result);
    }

    // =========================================================
    // STATISTICS
    // =========================================================

    private async Task<string> HandleStatisticsAsync(
        string message,
        CancellationToken cancellationToken)
    {
        var tool =
            _toolRegistry.GetTool(
                "get_experiment_statistics");

        if (tool is null)
            return "ابزار دریافت آمار آزمایش ها در دسترس نیست.";

        var result =
            await tool.ExecuteAsync(
                "{}",
                cancellationToken);

        return FormatStatisticsResult(
            result,
            message);
    }

    // =========================================================
    // PROJECT EXPERIMENTS
    // =========================================================

    private async Task<string> HandleProjectAsync(
        string message,
        CancellationToken cancellationToken)
    {
        var projectName =
            ExtractProjectName(message);

        if (string.IsNullOrWhiteSpace(projectName))
            return "لطفاً نام پروژه را مشخص کنید.";

        var tool =
            _toolRegistry.GetTool(
                "get_experiments");

        if (tool is null)
            return "ابزار دریافت آزمایش های پروژه در دسترس نیست.";

        var arguments =
            JsonSerializer.Serialize(new
            {
                projectName
            });

        var result =
            await tool.ExecuteAsync(
                arguments,
                cancellationToken);

        return FormatProjectResult(
            result,
            projectName);
    }

    // =========================================================
    // SPECIFIC EXPERIMENT
    // =========================================================

    private async Task<string> HandleSpecificAsync(
        string message,
        CancellationToken cancellationToken)
    {
        var title =
            ExtractExperimentTitle(message);

        if (string.IsNullOrWhiteSpace(title))
            return "لطفاً عنوان آزمایش را مشخص کنید.";

        var tool =
            _toolRegistry.GetTool(
                "get_experiment");

        if (tool is null)
            return "ابزار دریافت اطلاعات آزمایش در دسترس نیست.";

        var arguments =
            JsonSerializer.Serialize(new
            {
                title
            });

        var result =
            await tool.ExecuteAsync(
                arguments,
                cancellationToken);

        return FormatSpecificResult(result);
    }

    // =========================================================
    // SEARCH
    // =========================================================

    private async Task<string> HandleSearchAsync(
        string message,
        CancellationToken cancellationToken)
    {
        var searchTerm =
            ExtractSearchTerm(message);

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return "لطفاً عبارت مورد نظر برای جستجوی آزمایش را مشخص کنید.";
        }

        var tool =
            _toolRegistry.GetTool(
                "search_experiments");

        if (tool is null)
            return "ابزار جستجوی آزمایش ها در دسترس نیست.";

        var arguments =
            JsonSerializer.Serialize(new
            {
                searchTerm
            });

        var result =
            await tool.ExecuteAsync(
                arguments,
                cancellationToken);

        return FormatSearchResult(
            result,
            searchTerm);
    }

    // =========================================================
    // DETECTION
    // =========================================================

    private static bool IsLatestRequest(
        string message)
    {
        var text =
            message.Trim().ToLowerInvariant();

        var keywords = new[]
        {
            "آخرین آزمایش",
            "جدیدترین آزمایش",
            "آخرین آزمایش ثبت شده",
            "جدیدترین آزمایش ثبت شده",
            "last experiment",
            "latest experiment",
            "most recent experiment",
            "newest experiment"
        };

        return keywords.Any(
            x => text.Contains(x));
    }

    private static bool IsStatisticsRequest(
        string message)
    {
        var text =
            message.Trim().ToLowerInvariant();

        var keywords = new[]
        {
            "آمار آزمایش",
            "آمار آزمایش ها",
            "آمار آزمایش‌ها",
            "آمار آزمایشات",
            "تعداد آزمایش",
            "تعداد آزمایش ها",
            "تعداد آزمایش‌ها",
            "چند تا آزمایش",
            "چند آزمایش",
            "آزمایش تکمیل شده",
            "آزمایش‌های تکمیل شده",
            "آزمایش های تکمیل شده",
            "آزمایش تکمیل‌شده",
            "آزمایش در حال انجام",
            "آزمایش‌های در حال انجام",
            "آزمایش های در حال انجام",
            "آزمایش شروع نشده",
            "آزمایش‌های شروع نشده",
            "آزمایش های شروع نشده",
            "experiment statistics",
            "experiments statistics",
            "how many experiments",
            "completed experiments",
            "in progress experiments",
            "not started experiments"
        };

        return keywords.Any(
            x => text.Contains(x));
    }

    private static bool IsProjectRequest(
        string message)
    {
        var text =
            message.Trim().ToLowerInvariant();

        return
            text.Contains("آزمایش های پروژه") ||
            text.Contains("آزمایش‌های پروژه") ||
            text.Contains("آزمایش پروژه") ||
            text.Contains("experiments of project") ||
            text.Contains("experiments in project");
    }

    private static bool IsSpecificRequest(
        string message)
    {
        var text =
            message.Trim().ToLowerInvariant();

        var markers = new[]
        {
            "اطلاعات آزمایش",
            "جزئیات آزمایش",
            "درباره آزمایش",
            "اطلاعات experiment",
            "جزئیات experiment",
            "experiment info",
            "experiment details"
        };

        return markers.Any(
            x => text.Contains(x));
    }

    // =========================================================
    // EXTRACTION
    // =========================================================

    private static string? ExtractProjectName(
        string message)
    {
        var markers = new[]
        {
            "آزمایش های پروژه",
            "آزمایش‌های پروژه",
            "آزمایش پروژه",
            "experiments of project",
            "experiments in project"
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
            "اطلاعات آزمایش",
            "جزئیات آزمایش",
            "درباره آزمایش",
            "آزمایش",
            "experiment"
        };

        return ExtractAfterMarker(
            message,
            markers);
    }

    private static string? ExtractSearchTerm(
        string message)
    {
        var markers = new[]
        {
            "مربوط به",
            "مرتبط با",
            "برای",
            "آزمایش های",
            "آزمایش‌های",
            "آزمایش ها",
            "آزمایش‌ها",
            "آزمایش",
            "experiments related to",
            "experiments for",
            "experiment"
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
                    '\'');

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
    // FORMATTERS
    // =========================================================

    private static string FormatLatestResult(
        string toolResult)
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
                    out var result)
                    ? result
                    : root;

            if (!data.TryGetProperty(
                    "experiment",
                    out var experiment) ||
                experiment.ValueKind == JsonValueKind.Null)
            {
                return "آخرین آزمایش ثبت شده ای پیدا نشد.";
            }

            return FormatExperimentObject(
                experiment,
                "آخرین آزمایش ثبت شده:");
        }
        catch (JsonException)
        {
            return "در پردازش اطلاعات آخرین آزمایش مشکلی پیش آمد.";
        }
    }

    private static string FormatStatisticsResult(
        string toolResult,
        string message)
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
                    out var result)
                    ? result
                    : root;

            var total =
                GetInt(data, "total");

            var completed =
                GetInt(data, "completed");

            var inProgress =
                GetInt(data, "inProgress");

            var notStarted =
                GetInt(data, "notStarted");

            var text =
                message.Trim().ToLowerInvariant();

            if (text.Contains("تکمیل شده") ||
                text.Contains("تکمیل‌شده") ||
                text.Contains("completed"))
            {
                return
                    $"تعداد آزمایش های تکمیل شده: {completed}";
            }

            if (text.Contains("در حال انجام") ||
                text.Contains("in progress"))
            {
                return
                    $"تعداد آزمایش های در حال انجام: {inProgress}";
            }

            if (text.Contains("شروع نشده") ||
                text.Contains("not started"))
            {
                return
                    $"تعداد آزمایش های شروع نشده: {notStarted}";
            }

            if (text.Contains("چند تا آزمایش") ||
                text.Contains("چند آزمایش") ||
                text.Contains("how many experiments") ||
                text.Contains("تعداد آزمایش"))
            {
                return
                    $"تعداد کل آزمایش ها: {total}";
            }

            return
                $"آمار آزمایش ها:\n" +
                $"- مجموع آزمایش ها: {total}\n" +
                $"- تکمیل شده: {completed}\n" +
                $"- در حال انجام: {inProgress}\n" +
                $"- شروع نشده: {notStarted}";
        }
        catch (JsonException)
        {
            return "در پردازش آمار آزمایش ها مشکلی پیش آمد.";
        }
    }

    private static string FormatSpecificResult(
        string toolResult)
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
                    out var result)
                    ? result
                    : root;

            if (!data.TryGetProperty(
                    "experiment",
                    out var experiment) ||
                experiment.ValueKind == JsonValueKind.Null)
            {
                return "آزمایش مورد نظر پیدا نشد.";
            }

            return FormatExperimentObject(
                experiment,
                "اطلاعات آزمایش:");
        }
        catch (JsonException)
        {
            return "در پردازش اطلاعات آزمایش مشکلی پیش آمد.";
        }
    }

    private static string FormatProjectResult(
        string toolResult,
        string projectName)
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
                    out var result)
                    ? result
                    : root;

            if (data.ValueKind == JsonValueKind.Null)
                return $"پروژه «{projectName}» پیدا نشد.";

            if (data.TryGetProperty(
                    "found",
                    out var found) &&
                found.ValueKind == JsonValueKind.False)
            {
                return $"پروژه «{projectName}» پیدا نشد.";
            }

            var count =
                GetInt(data, "count");

            if (count == 0)
            {
                return
                    $"برای پروژه «{projectName}» آزمایشی ثبت نشده است.";
            }

            if (!data.TryGetProperty(
                    "experiments",
                    out var experiments))
            {
                return
                    $"برای پروژه «{projectName}» آزمایشی پیدا نشد.";
            }

            var actualName =
                data.TryGetProperty(
                    "projectName",
                    out var name)
                    ? name.GetString()
                    : projectName;

            var response =
                new StringBuilder();

            response.AppendLine(
                $"آزمایش های پروژه «{actualName}»:");

            var index = 1;

            foreach (var experiment in
                     experiments.EnumerateArray())
            {
                var title =
                    GetString(
                        experiment,
                        "title");

                var startedAt =
                    GetString(
                        experiment,
                        "startedAt");

                var completedAt =
                    GetString(
                        experiment,
                        "completedAt");

                var status =
                    !string.IsNullOrWhiteSpace(completedAt)
                        ? "تکمیل شده"
                        : !string.IsNullOrWhiteSpace(startedAt)
                            ? "در حال انجام"
                            : "شروع نشده";

                response.AppendLine(
                    $"{index}. {title ?? "بدون عنوان"}");

                response.AppendLine(
                    $"   وضعیت: {status}");

                index++;
            }

            return response
                .ToString()
                .Trim();
        }
        catch (JsonException)
        {
            return "در پردازش آزمایش های پروژه مشکلی پیش آمد.";
        }
    }

    private static string FormatSearchResult(
        string toolResult,
        string searchTerm)
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
                    out var result)
                    ? result
                    : root;

            var count =
                GetInt(data, "count");

            if (count == 0)
            {
                return
                    $"هیچ آزمایشی مرتبط با «{searchTerm}» پیدا نشد.";
            }

            if (!data.TryGetProperty(
                    "experiments",
                    out var experiments))
            {
                return
                    $"تعداد {count} آزمایش مرتبط با «{searchTerm}» پیدا شد.";
            }

            var response =
                new StringBuilder();

            response.AppendLine(
                $"تعداد آزمایش های مربوط به {searchTerm}: {count}");

            response.AppendLine();

            foreach (var experiment in
                     experiments.EnumerateArray())
            {
                var title =
                    GetString(
                        experiment,
                        "title");

                var description =
                    GetString(
                        experiment,
                        "description");

                response.AppendLine(
                    $"- عنوان آزمایش: {title ?? "نامشخص"}");

                if (!string.IsNullOrWhiteSpace(description))
                {
                    response.AppendLine(
                        $"  توضیح: {description}");
                }

                response.AppendLine();
            }

            return response
                .ToString()
                .Trim();
        }
        catch (JsonException)
        {
            return "در پردازش اطلاعات آزمایش ها مشکلی پیش آمد.";
        }
    }

    private static string FormatExperimentObject(
        JsonElement experiment,
        string header)
    {
        var response =
            new StringBuilder();

        response.AppendLine(header);

        AppendValue(
            experiment,
            "title",
            "عنوان",
            response);

        AppendValue(
            experiment,
            "description",
            "توضیح",
            response);

        AppendValue(
            experiment,
            "protocol",
            "پروتکل",
            response);

        AppendValue(
            experiment,
            "startedAt",
            "زمان شروع",
            response);

        AppendValue(
            experiment,
            "completedAt",
            "زمان پایان",
            response);

        AppendValue(
            experiment,
            "notes",
            "یادداشت",
            response);

        return response
            .ToString()
            .Trim();
    }

    private static void AppendValue(
        JsonElement element,
        string property,
        string label,
        StringBuilder response)
    {
        if (!element.TryGetProperty(
                property,
                out var value) ||
            value.ValueKind == JsonValueKind.Null)
        {
            return;
        }

        var text =
            value.ToString();

        if (!string.IsNullOrWhiteSpace(text))
        {
            response.AppendLine(
                $"- {label}: {text}");
        }
    }

    private static int GetInt(
        JsonElement element,
        string property)
    {
        return element.TryGetProperty(
                property,
                out var value) &&
            value.TryGetInt32(
                out var result)
            ? result
            : 0;
    }

    private static string? GetString(
        JsonElement element,
        string property)
    {
        if (!element.TryGetProperty(
                property,
                out var value) ||
            value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return value.ToString();
    }
}