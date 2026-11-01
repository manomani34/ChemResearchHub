using ChemResearchHub.Domain.Entities.WorkItemUserAlert;
using ChemResearchHub.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChemResearchHub.Infrastructure.Persistence.Configurations;

public class WorkItemUserAlertConfiguration
    : IEntityTypeConfiguration<WorkItemUserAlert>
{
    public void Configure(
        EntityTypeBuilder<WorkItemUserAlert> builder)
    {
        builder.ToTable("WorkItemUserAlerts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.WorkItemId)
            .IsRequired();

        builder.Property(x => x.ReadAt)
            .IsRequired(false);

        builder.HasOne<Domain.Entities.WorkItem.WorkItem>()
            .WithMany()
            .HasForeignKey(x => x.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.UserId,
            x.ReadAt
        });

        builder.HasIndex(x => new
        {
            x.WorkItemId,
            x.UserId
        })
        .IsUnique();
    }
}