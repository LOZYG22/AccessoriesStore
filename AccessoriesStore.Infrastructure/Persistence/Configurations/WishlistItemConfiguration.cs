using AccessoriesStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccessoriesStore.Infrastructure.Persistence.Configurations;

public class WishlistItemConfiguration
    : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.Property(w => w.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(w => w.CreatedAt)
            .IsRequired();

        builder.HasOne(w => w.Product)
            .WithMany(p => p.WishlistItems)
            .HasForeignKey(w => w.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => new
        {
            w.UserId,
            w.ProductId
        })
        .IsUnique();
    }
}