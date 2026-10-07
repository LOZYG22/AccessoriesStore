
namespace AccessoriesStore.Application.DTOs.Wishlist
{
    public class WishlistItemResponse
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string ProductSlug { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public decimal? DiscountPrice { get; set; }

        public string? MainImageUrl { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
