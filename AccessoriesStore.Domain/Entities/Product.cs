
namespace AccessoriesStore.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public decimal? DiscountPrice { get; set; }

        public string SKU { get; set; } = string.Empty;
        public int StockQuantity { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
        public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();

        public bool IsActive { get; set; } = true;

        public bool IsFeatured { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
