using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.ScheduleManagement.Domain.Entities;

namespace Zentry.Modules.ScheduleManagement.Infrastructure.Persistence.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.StudentId)
            .IsRequired();

        builder.Property(e => e.ScheduleId) // Changed from CourseId to ScheduleId based on DB design and logic
            .IsRequired();

        builder.Property(e => e.EnrolledAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasIndex(e => new { e.StudentId, e.ScheduleId })
            .IsUnique()
            .HasDatabaseName("IX_Enrollments_StudentId_ScheduleId");

        builder.HasIndex(e => e.StudentId);
        builder.HasIndex(e => e.ScheduleId);
    }
}