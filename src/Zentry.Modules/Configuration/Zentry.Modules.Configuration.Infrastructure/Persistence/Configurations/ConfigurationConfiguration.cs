using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Zentry.Modules.Configuration.Infrastructure.Persistence.Configurations;

public class ConfigurationConfiguration : IEntityTypeConfiguration<Domain.Entities.Configuration>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Configuration> builder)
    {
        builder.ToTable("Configurations");

        builder.Property(c => c.ConfigurationId)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(c => c.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Value)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.Description)
            .HasMaxLength(1000);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt);

        // Unique constraint trên Key
        builder.HasIndex(c => c.Key)
            .IsUnique()
            .HasDatabaseName("IX_Configurations_Key");

        // Nếu ConfigurationId và Id là giống nhau, có thể tạo unique index
        builder.HasIndex(c => c.ConfigurationId)
            .IsUnique()
            .HasDatabaseName("IX_Configurations_ConfigurationId");
    }
}