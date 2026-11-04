using ChemResearchHub.Domain.Entities.Meetings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChemResearchHub.Infrastructure.Persistence.Configurations.Meetings;

public class MeetingTemplateConfiguration
    : IEntityTypeConfiguration<MeetingTemplate>
{
    public void Configure(EntityTypeBuilder<MeetingTemplate> builder)
    {
        builder.ToTable("MeetingTemplates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.DefaultDurationMinutes)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ModifiedAt).IsRequired(false);

        builder.HasIndex(x => x.IsActive);
    }
}