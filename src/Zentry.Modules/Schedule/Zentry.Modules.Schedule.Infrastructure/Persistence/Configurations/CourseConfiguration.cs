using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.Schedule.Domain.Entities;

namespace Zentry.Modules.Schedule.Infrastructure.Persistence.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");

        builder.HasKey(c => c.CourseId);

        builder.Property(c => c.CourseId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(c => c.Code).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Semester).IsRequired().HasMaxLength(20);
        builder.Property(c => c.LecturerId).IsRequired();

        // Ignore property Id từ AggregateRoot
        builder.Ignore(c => c.Id);
    }
}