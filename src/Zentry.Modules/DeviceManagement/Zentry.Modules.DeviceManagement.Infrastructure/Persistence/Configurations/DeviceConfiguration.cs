using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.DeviceManagement.Domain.Entities;
using Zentry.Modules.DeviceManagement.Domain.Enums;
using Zentry.Modules.DeviceManagement.Domain.ValueObjects;

namespace Zentry.Modules.DeviceManagement.Infrastructure.Persistence.Configurations;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("Devices");

        builder.HasKey(d => d.DeviceId);
        builder.Property(d => d.AccountId).IsRequired();
        builder.Property(d => d.DeviceName)
            .HasConversion(n => n.Value, v => DeviceName.Create(v))
            .HasMaxLength(255)
            .IsRequired();
        builder.Property(d => d.DeviceToken)
            .HasConversion(t => t.Value, v => DeviceToken.Create())
            .HasMaxLength(255)
            .IsRequired();
        builder.HasIndex(d => d.DeviceToken).IsUnique();
        builder.Property(d => d.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(d => d.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAddOrUpdate();
        builder.Property(d => d.LastVerifiedAt);
        builder.Property(d => d.Status)
            .HasConversion(s => s.ToString(), n => DeviceStatus.FromName(n))
            .HasDefaultValueSql("'Active'");
        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.AccountId);
    }
}