
namespace AccessoriesStore.Application.DTOs.ProductReviews
{
    public class ProductReviewResponse
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public bool IsMine { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public string UserId { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;
        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
