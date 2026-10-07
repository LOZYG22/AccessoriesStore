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
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.Property(r => r.TokenHash)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(r => r.UserId)
                .HasMaxLength(450)
                .IsRequired();

            builder.HasIndex(r => r.TokenHash)
                .IsUnique();

            builder.HasIndex(r => r.UserId);

            builder.Property(r => r.ExpiresAt)
                .IsRequired();

            builder.Property(r => r.CreatedAt)
                .IsRequired();
        }
    }
}
