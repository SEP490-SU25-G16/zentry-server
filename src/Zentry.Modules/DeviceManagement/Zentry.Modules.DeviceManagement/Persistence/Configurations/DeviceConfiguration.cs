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
            .ValueGeneratedOnAdd();

        builder.Property(d => d.UserId).IsRequired();

        builder.Property(d => d.DeviceName)
            .HasConversion(n => n.Value, v => DeviceName.Create(v))
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(d => d.DeviceToken)
            .HasConversion(t => t.Value, v => DeviceToken.Create())
            .HasMaxLength(255)
            .IsRequired();

        // Cấu hình MAC Address
        builder.Property(d => d.MacAddress)
            .HasConversion(
                m => m.Value,
                v => MacAddress.Create(v))
            .HasMaxLength(17) // AA:BB:CC:DD:EE:FF = 17 characters
            .IsRequired();

        // Index cho các trường quan trọng
        builder.HasIndex(d => d.DeviceToken).IsUnique();
        builder.HasIndex(d => d.MacAddress).IsUnique(); // MAC address cũng phải unique
        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.UserId);

        // Composite index cho việc tìm kiếm theo user và MAC
        builder.HasIndex(d => new { d.UserId, d.MacAddress })
            .HasDatabaseName("IX_Devices_UserId_MacAddress");

        // Composite index cho việc tìm kiếm theo MAC và status (cho bluetooth scanning)
        builder.HasIndex(d => new { d.MacAddress, d.Status })
            .HasDatabaseName("IX_Devices_MacAddress_Status");

        builder.Property(d => d.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(d => d.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAddOrUpdate();
        builder.Property(d => d.LastVerifiedAt);
        builder.Property(d => d.Status)
            .HasConversion(s => s.ToString(), n => DeviceStatus.FromName(n))
            .HasDefaultValueSql("'Active'");

        // Cấu hình các trường optional
        builder.Property(d => d.Platform).HasMaxLength(50);
        builder.Property(d => d.OsVersion).HasMaxLength(50);
        builder.Property(d => d.Model).HasMaxLength(100);
        builder.Property(d => d.Manufacturer).HasMaxLength(100);
        builder.Property(d => d.AppVersion).HasMaxLength(50);
        builder.Property(d => d.PushNotificationToken).HasMaxLength(500);
    }
}