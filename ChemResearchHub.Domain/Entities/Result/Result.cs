using ChemResearchHub.Domain.Common;

namespace ChemResearchHub.Domain.Entities.Result;

public class Result : BaseEntity
{
    private Result()
    {
    }

    public Result(
        int sampleId,
        string metricName)
    {
        if (sampleId <= 0)
        {
            throw new ArgumentException(
                "SampleId must be greater than zero.",
                nameof(sampleId));
        }

        SampleId = sampleId;

        SetMetricName(metricName);
    }

    public int SampleId { get; private set; }

    public string MetricName { get; private set; } = null!;

    public decimal? NumericValue { get; private set; }

    public string? TextValue { get; private set; }

    public string? Unit { get; private set; }

    public string? Method { get; private set; }

    public string Status { get; private set; } = "Pending";

    public string? Evidence { get; private set; }

    public string? Notes { get; private set; }

    public void Update(
        string metricName,
        decimal? numericValue,
        string? textValue,
        string? unit,
        string? method,
        string status,
        string? evidence,
        string? notes)
    {
        SetMetricName(metricName);

        NumericValue = numericValue;
        TextValue = textValue;
        Unit = unit;
        Method = method;
        Status = string.IsNullOrWhiteSpace(status)
            ? "Pending"
            : status.Trim();
        Evidence = evidence;
        Notes = notes;

        ModifiedAt = DateTime.UtcNow;
    }

    private void SetMetricName(string metricName)
    {
        if (string.IsNullOrWhiteSpace(metricName))
        {
            throw new ArgumentException(
                "Metric name is required.",
                nameof(metricName));
        }

        MetricName = metricName.Trim();
    }
}