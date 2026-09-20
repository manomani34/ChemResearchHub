using ChemResearchHub.Domain.Entities.Experiment;
using ChemResearchHub.Domain.Entities.Sample;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChemResearchHub.Infrastructure.Persistence.Configurations;

public class SampleConfiguration
    : IEntityTypeConfiguration<Sample>
{
    public void Configure(
        EntityTypeBuilder<Sample> builder)
    {
        builder.ToTable("Samples");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ExperimentId)
            .IsRequired();

        builder.Property(x => x.SampleCode)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Name)
            .HasMaxLength(300)
            .IsRequired(false);

        builder.Property(x => x.SampleType)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(x => x.Matrix)
            .HasMaxLength(300)
            .IsRequired(false);

        builder.Property(x => x.PreparationMethod)
            .HasMaxLength(5000)
            .IsRequired(false);

        builder.Property(x => x.CollectedAt)
            .IsRequired(false);

        builder.Property(x => x.ExternalReference)
            .HasMaxLength(300)
            .IsRequired(false);

        builder.Property(x => x.Description)
            .HasMaxLength(5000)
            .IsRequired(false);

        builder.Property(x => x.Notes)
            .HasMaxLength(10000)
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.ModifiedAt)
            .IsRequired(false);

        builder.HasOne<Experiment>()
            .WithMany()
            .HasForeignKey(x => x.ExperimentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ExperimentId);

        builder.HasIndex(x => x.SampleCode);
    }
}