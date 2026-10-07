using AccessoriesStore.Application.DTOs.Carts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.Abstractions.Carts
{
    public interface ICartService
    {
        Task<CartResponse> GetCartAsync(string userId);

        Task<CartResponse> AddToCartAsync(
            string userId,
            int productId,
            int quantity);

        Task<CartResponse> UpdateQuantityAsync(
            string userId,
            int productId,
            int quantity);

        Task RemoveFromCartAsync(
            string userId,
            int productId);

        Task ClearCartAsync(string userId);
    }
}
