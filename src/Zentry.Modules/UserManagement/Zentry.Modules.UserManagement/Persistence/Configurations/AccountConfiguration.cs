using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.UserManagement.Persistence.Entities;

namespace Zentry.Modules.UserManagement.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Email).IsRequired().HasMaxLength(255);
        builder.HasIndex(a => a.Email).IsUnique(); // Ensure email is unique
        builder.Property(a => a.PasswordHash).IsRequired().HasMaxLength(255);
        builder.Property(a => a.PasswordSalt).IsRequired().HasMaxLength(255);
        builder.Property(a => a.Role).IsRequired().HasMaxLength(50);

        // Configure Password Reset Token properties
        builder.Property(a => a.ResetToken).HasMaxLength(255);
        builder.Property(a => a.ResetTokenExpiryTime);

        // Optional: Add a unique index on ResetToken for non-null values
        // This ensures no two *active* reset tokens are the same across accounts
        builder.HasIndex(a => a.ResetToken)
            .IsUnique()
            .HasFilter("\"ResetToken\" IS NOT NULL"); // For PostgreSQL
        // For SQL Server: .HasFilter("[ResetToken] IS NOT NULL");
    }
}
