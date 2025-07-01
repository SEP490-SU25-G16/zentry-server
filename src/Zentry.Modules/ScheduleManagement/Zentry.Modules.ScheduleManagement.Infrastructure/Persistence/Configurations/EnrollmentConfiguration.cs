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

        builder.Property(e => e.ScheduleId)
            .IsRequired();

        builder.Property(e => e.EnrolledAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Configure Relationships
        builder.HasOne(e => e.Schedule)
            .WithMany()
            .HasForeignKey(e => e.ScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.StudentId, e.ScheduleId })
            .IsUnique()
            .HasDatabaseName("IX_Enrollments_StudentId_ScheduleId");

        builder.HasIndex(e => e.StudentId);
        builder.HasIndex(e => e.ScheduleId);
    }
}