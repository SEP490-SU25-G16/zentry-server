using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.ScheduleManagement.Domain.Entities;

namespace Zentry.Modules.ScheduleManagement.Infrastructure.Persistence.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedOnAdd();

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(c => c.IsDeleted) // Cấu hình IsDeleted
            .IsRequired()
            .HasDefaultValue(false); // Mặc định là false

        // Unique index for Code
        builder.HasIndex(c => c.Code)
            .IsUnique();

        // Global Query Filter: Đảm bảo chỉ trả về các bản ghi IsDeleted = false
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
