using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.AttendanceManagement.Domain.Entities;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Persistence.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments");

        // Đặt thuộc tính Id kế thừa làm khóa chính
        builder.HasKey(d => d.Id);

        // Cấu hình thuộc tính Id
        builder.Property(d => d.Id)
            .ValueGeneratedOnAdd(); // Đảm bảo Id được tạo khi thêm mới

        builder.Property(e => e.StudentId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(e => e.CourseId)
            .HasColumnType("uuid")
            .IsRequired();

        // Add unique constraint to prevent duplicate enrollments
        builder.HasIndex(e => new { e.StudentId, e.CourseId })
            .IsUnique()
            .HasDatabaseName("IX_Enrollments_StudentId_CourseId");

        // Add indexes for performance
        builder.HasIndex(e => e.StudentId);
        builder.HasIndex(e => e.CourseId);
    }
}
