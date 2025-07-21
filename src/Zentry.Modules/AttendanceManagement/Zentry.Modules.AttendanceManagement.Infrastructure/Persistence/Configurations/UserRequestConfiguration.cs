using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.Modules.AttendanceManagement.Domain.Enums;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Persistence.Configurations;

public class UserRequestConfiguration : IEntityTypeConfiguration<UserRequest>
{
    public void Configure(EntityTypeBuilder<UserRequest> builder)
    {
        builder.ToTable("UserRequests");

        builder.HasKey(ur => ur.Id);

        builder.Property(ur => ur.Id)
            .ValueGeneratedOnAdd();

        builder.Property(ur => ur.RequestedByUserId)
            .IsRequired();

        builder.Property(ur => ur.TargetUserId)
            .IsRequired();

        builder.Property(ur => ur.RequestType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ur => ur.RelatedEntityId)
            .IsRequired();

        builder.Property(ur => ur.Status)
            .HasConversion(
                s => s.ToString(),
                s => UserRequestStatus.FromName(s)
            )
            .IsRequired()
            .HasMaxLength(50); // Max length for enum string

        builder.Property(ur => ur.Reason)
            .HasMaxLength(500); // Optional reason

        builder.Property(ur => ur.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(ur => ur.ProcessedAt); // Nullable

        builder.HasIndex(ur => ur.RequestedByUserId);
        builder.HasIndex(ur => ur.TargetUserId);
        builder.HasIndex(ur => ur.RequestType);
        builder.HasIndex(ur => ur.Status);
    }
}
