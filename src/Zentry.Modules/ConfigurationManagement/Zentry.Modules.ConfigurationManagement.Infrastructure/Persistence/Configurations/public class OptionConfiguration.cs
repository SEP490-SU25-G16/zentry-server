using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zentry.Modules.ConfigurationManagement.Domain.Entities;

namespace Zentry.Modules.ConfigurationManagement.Infrastructure.Persistence.Configurations;


public class OptionConfiguration : IEntityTypeConfiguration<Option>
{
    public void Configure(EntityTypeBuilder<Option> builder)
    {
        builder.ToTable("Options");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .ValueGeneratedOnAdd();

        builder.Property(o => o.AttributeId)
            .IsRequired();

        builder.Property(o => o.Value)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(o => o.DisplayLabel)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(o => o.SortOrder)
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAddOrUpdate();

        builder.HasIndex(o => o.AttributeId);
        builder.HasIndex(o => o.SortOrder);

        builder.HasIndex(o => new { o.AttributeId, o.Value })
            .IsUnique(); // A unique option value per attribute
    }
}
