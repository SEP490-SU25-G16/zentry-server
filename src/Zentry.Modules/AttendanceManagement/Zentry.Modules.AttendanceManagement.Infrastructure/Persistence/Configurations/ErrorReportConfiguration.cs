using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.AttendanceManagement.Domain.Entities;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Persistence.Configurations;

public class ErrorReportConfiguration : IEntityTypeConfiguration<ErrorReport>
{
    public void Configure(EntityTypeBuilder<ErrorReport> builder)
    {
        builder.ToTable("ErrorReports");

        // Đặt thuộc tính Id kế thừa làm khóa chính
        builder.HasKey(d => d.Id);

        // Cấu hình thuộc tính Id
        builder.Property(d => d.Id)
            .ValueGeneratedOnAdd(); // Đảm bảo Id được tạo khi thêm mới

        builder.Property(er => er.DeviceId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(er => er.ErrorCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(er => er.Description)
            .HasMaxLength(500);

        builder.Property(er => er.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        // Add indexes for performance
        builder.HasIndex(er => er.DeviceId);
        builder.HasIndex(er => er.ErrorCode);
        builder.HasIndex(er => er.CreatedAt);
    }
}
