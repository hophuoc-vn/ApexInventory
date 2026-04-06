using Apex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apex.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // 1. Primary Key
        builder.HasKey(p => p.Id);

        // 2. Property Constraints
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(50);

        // 3. Precision for Currency (Crucial for Finance/Banking!)
        builder.Property(p => p.Price)
            .HasPrecision(18, 2);

        // 4. Indexing for Performance
        builder.HasIndex(p => p.Sku)
            .IsUnique();

        // 5. Indexing for Search Speed
        builder.HasIndex(p => p.Name);
    }
}