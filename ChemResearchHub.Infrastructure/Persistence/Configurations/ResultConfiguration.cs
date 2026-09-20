using ChemResearchHub.Domain.Entities.Result;
using ChemResearchHub.Domain.Entities.Sample;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChemResearchHub.Infrastructure.Persistence.Configurations;

public class ResultConfiguration
    : IEntityTypeConfiguration<Result>
{
    public void Configure(
        EntityTypeBuilder<Result> builder)
    {
        builder.ToTable("Results");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.SampleId)
            .IsRequired();

        builder.Property(x => x.MetricName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.NumericValue)
            .HasPrecision(18, 6)
            .IsRequired(false);

        builder.Property(x => x.TextValue)
            .HasMaxLength(5000)
            .IsRequired(false);

        builder.Property(x => x.Unit)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(x => x.Method)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Evidence)
            .HasMaxLength(10000)
            .IsRequired(false);

        builder.Property(x => x.Notes)
            .HasMaxLength(10000)
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.ModifiedAt)
            .IsRequired(false);

        builder.HasOne<Sample>()
            .WithMany()
            .HasForeignKey(x => x.SampleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.SampleId);

        builder.HasIndex(x => x.MetricName);
    }
}