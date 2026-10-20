using AccessoriesStore.Domain.Entities;
using AccessoriesStore.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Address> Addresses => Set<Address>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
        public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
        public DbSet<StoreNotification> Notifications => Set<StoreNotification>();
        public DbSet<SupportConversation> SupportConversations => Set<SupportConversation>();
        public DbSet<SupportMessage> SupportMessages => Set<SupportMessage>();
        public DbSet<PaymentAttempt> PaymentAttempts => Set<PaymentAttempt>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(ApplicationDbContext).Assembly);

            modelBuilder.Entity<StoreNotification>(entity =>
            {
                entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
                entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
                entity.Property(x => x.Message).HasMaxLength(1000).IsRequired();
                entity.Property(x => x.Type).HasMaxLength(40).IsRequired();
                entity.Property(x => x.Link).HasMaxLength(500);
                entity.HasIndex(x => new { x.UserId, x.ReadAt, x.CreatedAt });
            });

            modelBuilder.Entity<SupportConversation>(entity =>
            {
                entity.Property(x => x.CustomerId).HasMaxLength(450).IsRequired();
                entity.Property(x => x.Subject).HasMaxLength(120).IsRequired();
                entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
                entity.HasIndex(x => new { x.CustomerId, x.UpdatedAt });
                entity.HasIndex(x => new { x.Status, x.UpdatedAt });
            });

            modelBuilder.Entity<SupportMessage>(entity =>
            {
                entity.Property(x => x.SenderId).HasMaxLength(450).IsRequired();
                entity.Property(x => x.SenderName).HasMaxLength(120).IsRequired();
                entity.Property(x => x.Body).HasMaxLength(4000).IsRequired();
                entity.HasOne(x => x.Conversation).WithMany(x => x.Messages)
                    .HasForeignKey(x => x.ConversationId).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(x => new { x.ConversationId, x.CreatedAt });
                entity.HasIndex(x => new { x.ConversationId, x.IsReadByAdmin, x.IsAdmin });
                entity.HasIndex(x => new { x.ConversationId, x.IsReadByCustomer, x.IsAdmin });
            });

            modelBuilder.Entity<PaymentAttempt>(entity =>
            {
                entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
                entity.Property(x => x.PaymentIntentId).HasMaxLength(100);
                entity.HasIndex(x => new { x.OrderId, x.CreatedAt });
                entity.HasIndex(x => x.PaymobOrderId);
                entity.HasIndex(x => x.PaymentIntentId).IsUnique().HasFilter("[PaymentIntentId] IS NOT NULL");
                entity.HasOne<Order>().WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
