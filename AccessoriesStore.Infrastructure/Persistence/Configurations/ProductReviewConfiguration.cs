using AccessoriesStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccessoriesStore.Infrastructure.Persistence.Configurations
{
    public class ProductReviewConfiguration
    : IEntityTypeConfiguration<ProductReview>
    {
        public void Configure(EntityTypeBuilder<ProductReview> builder)
        {
            builder.Property(r => r.UserId)
                .HasMaxLength(450)
                .IsRequired();

            builder.Property(r => r.Rating)
                .IsRequired();

            builder.Property(r => r.Comment)
                .HasMaxLength(1000);

            builder.Property(r => r.CreatedAt)
                .IsRequired();

            builder.HasIndex(r => new
            {
                r.ProductId,
                r.UserId
            })
            .IsUnique();

            builder.HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
