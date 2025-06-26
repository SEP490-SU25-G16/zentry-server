using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.ConfigurationManagement.Domain.Entities;
using Zentry.Modules.ConfigurationManagement.Domain.Enums;

namespace Zentry.Modules.ConfigurationManagement.Infrastructure.Persistence.Configurations;


public class AttributeDefinitionConfiguration : IEntityTypeConfiguration<AttributeDefinition>
{
    public void Configure(EntityTypeBuilder<AttributeDefinition> builder)
    {
        builder.ToTable("AttributeDefinitions");

        builder.HasKey(ad => ad.Id);

        builder.Property(ad => ad.Id)
            .ValueGeneratedOnAdd();

        builder.Property(ad => ad.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ad => ad.DisplayName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(ad => ad.Description)
            .HasMaxLength(1000);

        builder.Property(ad => ad.DataType)
            .HasConversion(
                dt => dt.ToString(),
                dt => DataType.FromName(dt)
            )
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ad => ad.ScopeType)
            .HasConversion(
                st => st.ToString(),
                st => ScopeType.FromName(st)
            )
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ad => ad.Unit)
            .HasMaxLength(50);

        builder.Property(ad => ad.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(ad => ad.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAddOrUpdate();

        builder.HasIndex(ad => ad.Key)
            .IsUnique();

        builder.HasIndex(ad => ad.DataType);
        builder.HasIndex(ad => ad.ScopeType);
    }
}
