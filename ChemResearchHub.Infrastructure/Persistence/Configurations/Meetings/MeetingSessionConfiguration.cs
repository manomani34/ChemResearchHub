using ChemResearchHub.Domain.Entities.Meetings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChemResearchHub.Infrastructure.Persistence.Configurations.Meetings;

public class MeetingSessionConfiguration : IEntityTypeConfiguration<MeetingSession>
{
    public void Configure(EntityTypeBuilder<MeetingSession> builder)
    {
        builder.ToTable("MeetingSessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.Description)
            .HasMaxLength(4000);

        builder.Property(x => x.ScheduledStartAt)
            .IsRequired();

        builder.Property(x => x.PlannedDurationMinutes)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.TimerState)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.FacilitatorUserId)
            .HasMaxLength(450);

        builder.Property(x => x.CreatedByUserId)
            .HasMaxLength(450);

        builder.Property(x => x.CurrentAgendaItemIndex)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ModifiedAt).IsRequired(false);

        builder.HasOne(x => x.MeetingSeries)
            .WithMany(x => x.Sessions)
            .HasForeignKey(x => x.MeetingSeriesId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.AgendaItems)
            .WithOne(x => x.MeetingSession)
            .HasForeignKey(x => x.MeetingSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Participants)
            .WithOne(x => x.MeetingSession)
            .HasForeignKey(x => x.MeetingSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ParkingLotItems)
            .WithOne(x => x.MeetingSession)
            .HasForeignKey(x => x.MeetingSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Decisions)
            .WithOne(x => x.MeetingSession)
            .HasForeignKey(x => x.MeetingSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ActionItems)
            .WithOne(x => x.MeetingSession)
            .HasForeignKey(x => x.MeetingSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.TimerEvents)
            .WithOne(x => x.MeetingSession)
            .HasForeignKey(x => x.MeetingSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ScheduledStartAt);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.FacilitatorUserId);
    }
}