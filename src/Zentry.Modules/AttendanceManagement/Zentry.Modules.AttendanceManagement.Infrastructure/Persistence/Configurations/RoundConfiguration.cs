using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.AttendanceManagement.Domain.Entities;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Persistence.Configurations;

public class RoundConfiguration : IEntityTypeConfiguration<Round>
{
    [Obsolete("Obsolete")]
    public void Configure(EntityTypeBuilder<Round> builder)
    {
        builder.ToTable("Rounds");

        // Đặt thuộc tính Id kế thừa làm khóa chính
        builder.HasKey(d => d.Id);

        // Cấu hình thuộc tính Id
        builder.Property(d => d.Id)
            .ValueGeneratedOnAdd(); // Đảm bảo Id được tạo khi thêm mới

        builder.Property(r => r.ScheduleId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(r => r.StartTime)
            .IsRequired();

        builder.Property(r => r.EndTime)
            .IsRequired();

        // Add indexes for performance
        builder.HasIndex(r => r.ScheduleId);
        builder.HasIndex(r => r.StartTime);
        builder.HasIndex(r => r.EndTime);

        // Add constraint to ensure EndTime > StartTime
        builder.HasCheckConstraint("CK_Rounds_EndTime_After_StartTime",
            "\"EndTime\" > \"StartTime\"");
    }
}
