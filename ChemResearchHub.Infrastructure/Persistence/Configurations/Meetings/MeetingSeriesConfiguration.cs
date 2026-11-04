using ChemResearchHub.Domain.Entities.Meetings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChemResearchHub.Infrastructure.Persistence.Configurations.Meetings;

public class MeetingSeriesConfiguration : IEntityTypeConfiguration<MeetingSeries>
{
    public void Configure(EntityTypeBuilder<MeetingSeries> builder)
    {
        builder.ToTable("MeetingSeries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.DefaultDurationMinutes)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ModifiedAt).IsRequired(false);

        builder.HasMany(x => x.Sessions)
            .WithOne(x => x.MeetingSeries)
            .HasForeignKey(x => x.MeetingSeriesId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.IsActive);
    }
}