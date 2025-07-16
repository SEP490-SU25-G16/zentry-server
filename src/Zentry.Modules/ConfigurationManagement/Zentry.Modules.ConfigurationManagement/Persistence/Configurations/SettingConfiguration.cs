using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.ConfigurationManagement.Persistence.Entities;
using Zentry.Modules.ConfigurationManagement.Persistence.Enums;

namespace Zentry.Modules.ConfigurationManagement.Persistence.Configurations;

public class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.ToTable("Settings");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedOnAdd();

        builder.Property(c => c.AttributeId)
            .IsRequired();

        builder.Property(c => c.ScopeType)
            .HasConversion(
                st => st.ToString(),
                st => ScopeType.FromName(st)
            )
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.ScopeId)
            .IsRequired();

        builder.Property(c => c.Value)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAddOrUpdate();

        builder.HasIndex(c => c.AttributeId);
        builder.HasIndex(c => c.ScopeType);
        builder.HasIndex(c => c.ScopeId);

        // Add unique constraint for combination of AttributeId, ScopeType, ScopeId
        // This ensures a specific setting value exists only once for a given scope
        builder.HasIndex(c => new { c.AttributeId, c.ScopeType, c.ScopeId })
            .IsUnique();

        // THÊM CẤU HÌNH MỐI QUAN HỆ VỚI ATTRIBUTEDEFINITION
        // Mỗi  Setting có một AttributeDefinition
        builder.HasOne(c => c.AttributeDefinition) //  Setting có một AttributeDefinition
            .WithMany() // AttributeDefinition có thể có nhiều Settings (nếu bạn muốn AttributeDefinition biết các Configurations của nó, bạn sẽ cần thêm collection vào AttributeDefinition)
            .HasForeignKey(c => c.AttributeId) // Khóa ngoại là AttributeId
            .IsRequired(); // Bắt buộc phải có AttributeDefinition
    }
}
