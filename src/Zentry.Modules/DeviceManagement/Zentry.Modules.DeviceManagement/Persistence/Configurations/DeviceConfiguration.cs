using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.DeviceManagement.Entities;
using Zentry.Modules.DeviceManagement.ValueObjects;
using Zentry.SharedKernel.Constants.Device;

namespace Zentry.Modules.DeviceManagement.Persistence.Configurations;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("Devices");

        // Đặt thuộc tính Id kế thừa làm khóa chính
        builder.HasKey(d => d.Id);

        // Cấu hình thuộc tính Id
        builder.Property(d => d.Id)
            .ValueGeneratedOnAdd(); // Đảm bảo Id được tạo khi thêm mới

        builder.Property(d => d.UserId).IsRequired();
        builder.Property(d => d.DeviceName)
            .HasConversion(n => n.Value, v => DeviceName.Create(v))
            .HasMaxLength(255)
            .IsRequired();
        builder.Property(d => d.DeviceToken)
            .HasConversion(t => t.Value, v => DeviceToken.Create()) // DeviceToken.Create() không có tham số
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
        builder.HasIndex(d => d.UserId);
    }
}