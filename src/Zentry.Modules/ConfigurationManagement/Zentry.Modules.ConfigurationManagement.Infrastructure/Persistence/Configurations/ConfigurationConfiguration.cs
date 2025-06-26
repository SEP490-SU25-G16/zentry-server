using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Zentry.Modules.ConfigurationManagement.Infrastructure.Persistence.Configurations;

public class ConfigurationConfiguration : IEntityTypeConfiguration<Domain.Entities.Configuration>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Configuration> builder)
    {
        builder.ToTable("Configurations");

        // Đặt thuộc tính Id kế thừa làm khóa chính
        builder.HasKey(d => d.Id);

        // Cấu hình thuộc tính Id
        builder.Property(d => d.Id)
            .ValueGeneratedOnAdd(); // Đảm bảo Id được tạo khi thêm mới

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
    }
}
