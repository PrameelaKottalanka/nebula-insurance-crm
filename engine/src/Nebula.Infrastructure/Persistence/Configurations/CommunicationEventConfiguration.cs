using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nebula.Domain.Entities;

namespace Nebula.Infrastructure.Persistence.Configurations;

public class CommunicationEventConfiguration : IEntityTypeConfiguration<CommunicationEvent>
{
    public void Configure(EntityTypeBuilder<CommunicationEvent> builder)
    {
        builder.ToTable("CommunicationEvents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventType).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Direction).HasMaxLength(20);
        builder.Property(e => e.Subject).HasMaxLength(200);
        builder.Property(e => e.Body).HasMaxLength(4000);
        builder.Property(e => e.Outcome).HasMaxLength(1000);
        builder.Property(e => e.PrimaryEntityType).IsRequired().HasMaxLength(20);
        builder.Property(e => e.PrimaryEntityId).IsRequired();
        builder.Property(e => e.AuthoredByUserId).IsRequired();
        builder.Property(e => e.AuthoredByDisplayName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.RedactedByDisplayName).HasMaxLength(200);
        builder.Property(e => e.RedactionReason).HasMaxLength(500);
        builder.Property(e => e.IsRedacted).HasDefaultValue(false);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);
        builder.Property(e => e.CreatedByUserId).IsRequired();
        builder.Property(e => e.UpdatedByUserId).IsRequired();

        builder.Property(e => e.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        builder.HasIndex(e => new { e.PrimaryEntityType, e.PrimaryEntityId, e.OccurredAt })
            .HasDatabaseName("IX_CommunicationEvents_Entity_OccurredAt");

        builder.HasIndex(e => e.AuthoredByUserId)
            .HasDatabaseName("IX_CommunicationEvents_AuthoredByUserId");

        builder.HasIndex(e => e.FollowUpTaskId)
            .HasDatabaseName("IX_CommunicationEvents_FollowUpTaskId");
    }
}
