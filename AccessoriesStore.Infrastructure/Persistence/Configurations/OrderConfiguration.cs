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
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.Property(o => o.SubTotal)
                .HasPrecision(18, 2);

            builder.Property(o => o.ShippingCost)
                .HasPrecision(18, 2);

            builder.Property(o => o.Discount)
                .HasPrecision(18, 2);

            builder.Property(o => o.Total)
                .HasPrecision(18, 2);

            builder.Property(o => o.Notes)
                .HasMaxLength(1000);

            builder.HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
