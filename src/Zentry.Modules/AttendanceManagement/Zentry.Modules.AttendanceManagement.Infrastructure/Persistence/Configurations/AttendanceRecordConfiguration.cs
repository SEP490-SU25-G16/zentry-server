// Zentry.Modules.AttendanceManagement.Infrastructure/Persistence/Configurations/AttendanceRecordConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.AttendanceManagement.Domain.Entities;
using Zentry.SharedKernel.Constants.Attendance;

namespace Zentry.Modules.AttendanceManagement.Infrastructure.Persistence.Configurations;

public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("AttendanceRecords");

        builder.HasKey(ar => ar.Id);

        builder.Property(ar => ar.Id)
            .ValueGeneratedOnAdd();

        builder.Property(ar => ar.StudentId)
            .IsRequired();

        builder.Property(ar => ar.SessionId)
            .IsRequired();

        builder.Property(ar => ar.Status)
            .HasConversion(
                s => s.ToString(),
                s => AttendanceStatus.FromName(s)
            )
            .IsRequired();

        builder.Property(ar => ar.IsManual)
            .IsRequired();

        builder.Property(ar => ar.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(ar => ar.ExpiredAt)
            .IsRequired();

        builder.Property(ar => ar.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAddOrUpdate();

        builder.HasIndex(ar => ar.StudentId);
        builder.HasIndex(ar => ar.SessionId);
        builder.HasIndex(ar => ar.Status);
    }
}