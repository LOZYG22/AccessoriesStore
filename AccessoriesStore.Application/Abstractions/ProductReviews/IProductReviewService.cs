using AccessoriesStore.Application.DTOs.ProductReviews;

namespace AccessoriesStore.Application.Abstractions.ProductReviews
{
    public interface IProductReviewService
    {
        Task<ProductReviewResponse> CreateAsync(
            string userId,
            int productId,
            CreateProductReviewRequest request);

        Task<ProductReviewResponse> UpdateAsync(
            string userId,
            int reviewId,
            CreateProductReviewRequest request);

        Task DeleteAsync(
            string userId,
            int reviewId);

        Task<IEnumerable<ProductReviewResponse>> GetByProductIdAsync(
            int productId);
    }
}
