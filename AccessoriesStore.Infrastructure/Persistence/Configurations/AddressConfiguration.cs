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
    public class AddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.Property(a => a.FullName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(a => a.PhoneNumber)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(a => a.City)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(a => a.Area)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(a => a.Street)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(a => a.BuildingNumber)
                .HasMaxLength(20);

            builder.Property(a => a.ApartmentNumber)
                .HasMaxLength(20);

            builder.Property(a => a.AdditionalDetails)
                .HasMaxLength(500);
        }
    }
}
