using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.ScheduleManagement.Domain.Entities;

namespace Zentry.Modules.ScheduleManagement.Infrastructure.Persistence.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms");

        // Đặt thuộc tính Id kế thừa làm khóa chính
        builder.HasKey(d => d.Id);

        // Cấu hình thuộc tính Id
        builder.Property(d => d.Id)
            .ValueGeneratedOnAdd(); // Đảm bảo Id được tạo khi thêm mới

        builder.Property(r => r.Name).IsRequired().HasMaxLength(50);
        builder.Property(r => r.Building).IsRequired().HasMaxLength(50);
        builder.Property(r => r.Capacity).IsRequired();


        // Add indexes for performance
        builder.HasIndex(r => r.Name);
        builder.HasIndex(r => r.Building);
    }
}
