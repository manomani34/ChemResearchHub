using ChemResearchHub.Domain.Entities.Meetings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChemResearchHub.Infrastructure.Persistence.Configurations.Meetings;

public class MeetingActionItemConfiguration
    : IEntityTypeConfiguration<MeetingActionItem>
{
    public void Configure(EntityTypeBuilder<MeetingActionItem> builder)
    {
        builder.ToTable("MeetingActionItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.Description)
            .HasMaxLength(4000);

        builder.Property(x => x.AssignedToUserId)
            .HasMaxLength(450);

        builder.Property(x => x.AcceptanceCriteria)
            .HasMaxLength(2000);

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ModifiedAt).IsRequired(false);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.AssignedToUserId);
        builder.HasIndex(x => x.DueDate);
        builder.HasIndex(x => x.WorkItemId)
            .IsUnique()
            .HasFilter("[WorkItemId] IS NOT NULL");
    }
}