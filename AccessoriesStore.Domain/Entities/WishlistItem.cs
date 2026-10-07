
namespace AccessoriesStore.Domain.Entities
{
    public class WishlistItem
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public int ProductId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Product Product { get; set; } = null!;
    }
}
