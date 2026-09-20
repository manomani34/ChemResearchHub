using ChemResearchHub.Domain.Entities.Attachment;
using ChemResearchHub.Domain.Entities.WorkItem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChemResearchHub.Infrastructure.Persistence.Configurations;

public class AttachmentConfiguration
    : IEntityTypeConfiguration<Attachment>
{
    public void Configure(
        EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("Attachments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.WorkItemId)
            .IsRequired();

        builder.Property(x => x.OriginalFileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.StoredFileName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.FileSize)
            .IsRequired();

        builder.Property(x => x.StoragePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Description)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(x => x.UploadedByUserId)
            .HasMaxLength(450)
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.ModifiedAt)
            .IsRequired(false);

        builder.HasOne<WorkItem>()
            .WithMany()
            .HasForeignKey(x => x.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.WorkItemId);

        builder.HasIndex(x => x.StoredFileName);
    }
}