using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.UserManagement.Enums;
using Zentry.Modules.UserManagement.Persistence.Entities;
using Zentry.Modules.UserManagement.Persistence.Enums;

namespace Zentry.Modules.UserManagement.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

        // Đặt thuộc tính Id kế thừa làm khóa chính
        builder.HasKey(a => a.Id);
        // Cấu hình thuộc tính Id
        builder.Property(a => a.Id)
            .ValueGeneratedOnAdd(); // Đảm bảo Id được tạo khi thêm mới

        builder.Property(a => a.Email).IsRequired().HasMaxLength(255);
        builder.HasIndex(a => a.Email).IsUnique(); // Ensure email is unique
        builder.Property(a => a.PasswordHash).IsRequired().HasMaxLength(255);
        builder.Property(a => a.PasswordSalt).IsRequired().HasMaxLength(255);
        builder.Property(a => a.Role).IsRequired().HasMaxLength(50);

        // Cấu hình thuộc tính CreatedAt
        builder.Property(a => a.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        // Cấu hình thuộc tính UpdatedAt
        builder.Property(a => a.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAddOrUpdate();

        // SỬA LỖI: Cấu hình đúng cho AccountStatus (Enumeration tùy chỉnh)
        builder.Property(a => a.Status)
            .HasConversion(
                // Chuyển AccountStatus sang string khi lưu vào DB
                status => status.ToString(),
                // Chuyển string từ DB sang AccountStatus
                statusString => AccountStatus.FromName(statusString)
            )
            .IsRequired()
            // BỎ HasDefaultValue vì không thể set default value cho custom enumeration
            // Thay vào đó sẽ dùng HasDefaultValueSql với string value
            .HasDefaultValueSql("'Active'"); // Sử dụng SQL default value

        builder.HasIndex(a => a.Status); // Thêm index cho Status

        // Configure Password Reset Token properties
        builder.Property(a => a.ResetToken).HasMaxLength(255);
        builder.Property(a => a.ResetTokenExpiryTime);

        // Optional: Add a unique index on ResetToken for non-null values
        builder.HasIndex(a => a.ResetToken)
            .IsUnique()
            .HasFilter("\"ResetToken\" IS NOT NULL"); // For PostgreSQL
    }
}
