using AccessoriesStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Infrastructure.Persistence.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.Property(p => p.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(p => p.Slug)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(p => p.Description)
                .HasMaxLength(2000);

            builder.Property(p => p.Price)
                .HasPrecision(18, 2);

            builder.Property(p => p.DiscountPrice)
                .HasPrecision(18, 2);

            builder.Property(p => p.SKU)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(p => p.StockQuantity)
                .IsRequired();

            builder.HasIndex(p => p.Slug)
                .IsUnique();

            builder.HasIndex(p => p.SKU)
                .IsUnique();

            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.Images)
                .WithOne(i => i.Product)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
        
}
