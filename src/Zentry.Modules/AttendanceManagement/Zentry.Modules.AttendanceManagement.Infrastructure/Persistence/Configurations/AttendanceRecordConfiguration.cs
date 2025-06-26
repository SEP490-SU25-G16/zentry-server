using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.AttendanceManagement.Domain.Entities;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Persistence.Configurations;

public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("AttendanceRecords");

        // Đặt thuộc tính Id kế thừa làm khóa chính
        builder.HasKey(d => d.Id);

        // Cấu hình thuộc tính Id
        builder.Property(d => d.Id)
            .ValueGeneratedOnAdd(); // Đảm bảo Id được tạo khi thêm mới

        builder.Property(ar => ar.EnrollmentId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(ar => ar.RoundId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(ar => ar.IsPresent).IsRequired();
        builder.Property(ar => ar.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        // Add unique constraint to prevent duplicate attendance records
        builder.HasIndex(ar => new { ar.EnrollmentId, ar.RoundId })
            .IsUnique()
            .HasDatabaseName("IX_AttendanceRecords_EnrollmentId_RoundId");

        // Add indexes for performance
        builder.HasIndex(ar => ar.EnrollmentId);
        builder.HasIndex(ar => ar.RoundId);

        builder.HasOne(ar => ar.Enrollment)
            .WithMany(e => e.AttendanceRecords)
            .HasForeignKey(ar => ar.EnrollmentId)
            .HasPrincipalKey(e => e.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ar => ar.Round)
            .WithMany(r => r.AttendanceRecords)
            .HasForeignKey(ar => ar.RoundId).HasPrincipalKey(r => r.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
