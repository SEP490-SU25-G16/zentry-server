using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.Attendance.Domain.Entities;

namespace Zentry.Modules.Attendance.Infrastructure.Persistence.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments");

        // Use EnrollmentId as primary key
        builder.HasKey(e => e.EnrollmentId);

        builder.Property(e => e.EnrollmentId)
            .HasColumnType("uuid")
            .IsRequired();

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