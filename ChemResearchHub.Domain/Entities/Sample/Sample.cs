using ChemResearchHub.Domain.Common;

namespace ChemResearchHub.Domain.Entities.Sample;

public class Sample : BaseEntity
{
    private Sample()
    {
    }

    public Sample(
        int experimentId,
        string sampleCode)
    {
        if (experimentId <= 0)
        {
            throw new ArgumentException(
                "ExperimentId must be greater than zero.",
                nameof(experimentId));
        }

        ExperimentId = experimentId;

        SetSampleCode(sampleCode);
    }

    public int ExperimentId { get; private set; }

    public string SampleCode { get; private set; } = null!;

    public string? Name { get; private set; }

    public string? SampleType { get; private set; }

    public string? Matrix { get; private set; }

    public string? PreparationMethod { get; private set; }

    public DateTime? CollectedAt { get; private set; }

    public string? ExternalReference { get; private set; }

    public string? Description { get; private set; }

    public string? Notes { get; private set; }

    public void Update(
        string sampleCode,
        string? name,
        string? sampleType,
        string? matrix,
        string? preparationMethod,
        DateTime? collectedAt,
        string? externalReference,
        string? description,
        string? notes)
    {
        SetSampleCode(sampleCode);

        Name = name;
        SampleType = sampleType;
        Matrix = matrix;
        PreparationMethod = preparationMethod;
        CollectedAt = collectedAt;
        ExternalReference = externalReference;
        Description = description;
        Notes = notes;

        ModifiedAt = DateTime.UtcNow;
    }

    private void SetSampleCode(string sampleCode)
    {
        if (string.IsNullOrWhiteSpace(sampleCode))
        {
            throw new ArgumentException(
                "Sample code is required.",
                nameof(sampleCode));
        }

        SampleCode = sampleCode.Trim();
    }
}