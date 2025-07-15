using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.AttendanceManagement.Domain.Entities;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Persistence.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    [Obsolete("Obsolete")]
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("Sessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd();

        builder.Property(s => s.ScheduleId)
            .IsRequired();

        builder.Property(s => s.UserId)
            .IsRequired();

        builder.Property(s => s.StartTime)
            .IsRequired();

        builder.Property(s => s.EndTime)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasIndex(s => s.ScheduleId);
        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.StartTime);

        builder.HasCheckConstraint("CK_Sessions_EndTime_After_StartTime",
            "\"EndTime\" > \"StartTime\"");

        // --- CẤU HÌNH CHO SESSIONCONFIGS (Value Object) ---
        // Sử dụng OwnsOne và ToJson để lưu Value Object vào một cột JSON duy nhất
        builder.OwnsOne(s => s.SessionConfigs, ownedBuilder =>
        {
            // Mặc định tên cột sẽ là "SessionConfigs", bạn có thể đổi thành "SessionConfigData" nếu muốn:
            // ownedBuilder.ToJson("SessionConfigData");
            ownedBuilder.ToJson(); // EF Core sẽ tự chọn tên cột và kiểu JSON phù hợp với DB
        });
    }
}