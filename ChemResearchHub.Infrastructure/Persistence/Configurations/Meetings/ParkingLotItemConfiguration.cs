using ChemResearchHub.Domain.Entities.Meetings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChemResearchHub.Infrastructure.Persistence.Configurations.Meetings;

public class ParkingLotItemConfiguration
    : IEntityTypeConfiguration<ParkingLotItem>
{
    public void Configure(EntityTypeBuilder<ParkingLotItem> builder)
    {
        builder.ToTable("MeetingParkingLotItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.Summary)
            .HasMaxLength(4000);

        builder.Property(x => x.RaisedByUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.AssignedToUserId)
            .HasMaxLength(450);

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Priority)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ModifiedAt).IsRequired(false);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.TargetMeetingSessionId);
        builder.HasIndex(x => x.ProjectId);
    }
}