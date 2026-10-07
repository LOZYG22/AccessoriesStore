using AccessoriesStore.Application.DTOs.Wishlist;

namespace AccessoriesStore.Application.Abstractions.Wishlist
{
    public interface IWishlistService
    {
        Task<WishlistItemResponse> AddAsync(
            string userId,
            AddToWishlistRequest request);

        Task RemoveAsync(
            string userId,
            int wishlistItemId);

        Task<IEnumerable<WishlistItemResponse>> GetMyWishlistAsync(
            string userId);
    }
}
