using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.ScheduleManagement.Domain.Entities;

namespace Zentry.Modules.ScheduleManagement.Infrastructure.Persistence.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");

        // Đặt thuộc tính Id kế thừa làm khóa chính
        builder.HasKey(d => d.Id);

        // Cấu hình thuộc tính Id
        builder.Property(d => d.Id)
            .ValueGeneratedOnAdd(); // Đảm bảo Id được tạo khi thêm mới

        builder.Property(c => c.Code).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Semester).IsRequired().HasMaxLength(20);
        builder.Property(c => c.LecturerId).IsRequired();

        // Dòng builder.Ignore(c => c.Id); đã được xóa ở lần sửa trước.
        // Đảm bảo nó không xuất hiện ở đây.
    }
}
