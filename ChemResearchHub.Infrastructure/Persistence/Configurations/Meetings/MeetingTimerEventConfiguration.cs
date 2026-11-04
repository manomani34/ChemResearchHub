using ChemResearchHub.Domain.Entities.Meetings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChemResearchHub.Infrastructure.Persistence.Configurations.Meetings;

public class MeetingTimerEventConfiguration
    : IEntityTypeConfiguration<MeetingTimerEvent>
{
    public void Configure(EntityTypeBuilder<MeetingTimerEvent> builder)
    {
        builder.ToTable("MeetingTimerEvents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.PerformedByUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.CorrelationId)
            .HasMaxLength(100);

        builder.Property(x => x.OccurredAt)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ModifiedAt).IsRequired(false);

        builder.HasIndex(x => new
        {
            x.MeetingSessionId,
            x.OccurredAt
        });

        builder.HasIndex(x => x.CorrelationId);
    }
}